import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, Subject, Subscription, takeUntil } from 'rxjs';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MessageDto } from '../../../dtos/MessageDto';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';

@Component({
  selector: 'app-prompt-box',
  imports: [FormsModule],
  templateUrl: './prompt-box.component.html',
  styleUrl: './prompt-box.component.scss',
})
export class PromptBoxComponent implements OnDestroy {
  constructor(
    public storeService: StoreService,
    private chatService: ChatService,
    private sanitizer: DomSanitizer
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });
  }

  prompt: string = '';
  message?: string = '';
  md: markdownit;
  sseSubscription: Subscription | undefined;
  private destroy$ = new Subject<void>();
  @Output() markdown = new EventEmitter<SafeHtml>();
  sanitizeHtml!: SafeHtml;

  async onClickCreateSession(): Promise<void> {
    if (!this.prompt.trim()) {
      return;
    }

    try {
      this.storeService.stream.set('');
      this.storeService.disablePromptButton.set(true);

      if (this.storeService.sessionId() === '') {
        const session = await firstValueFrom(this.chatService.createSession());
        this.storeService.sessionId.set(session.id);
      }

      this.storeService.messages.update((messages) => [
        ...messages,
        new MessageDto(this.prompt, true, undefined),
      ]);

      this.storeService.isStreaming.set(true);
      this.sseSubscription = this.chatService
        .getServerSentEvent(this.prompt)
        .subscribe({
          next: (message) => {
            this.storeService.stream.update((stream) => stream + message);
            const html = this.md.render(this.storeService.stream());
            this.sanitizeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
            this.markdown.emit(this.sanitizeHtml);
          },
          complete: () => {
            this.storeService.stream.set('');
            this.storeService.messages.update((messages) => [
              ...messages,
              new MessageDto('', false, this.sanitizeHtml),
            ]);
            this.storeService.disablePromptButton.set(false);
            this.storeService.isStreaming.set(false);
            this.storeService.streamMessage.set(
              new MessageDto('', false, undefined)
            );
          },
          error: (error) => {
            this.storeService.stream.set('');
            this.storeService.disablePromptButton.set(false);
            this.storeService.isStreaming.set(false);
            this.storeService.streamMessage.set(
              new MessageDto('', false, undefined)
            );
          },
        });

      this.prompt = '';
    } catch (error) {
      // Handle errors appropriately
      console.error('Error in session creation:', error);
      // You might want to show an error message to the user
    }
  }

  onModelChange(event: string): void {
    this.storeService.selectedModelId.set(event);
  }

  ngOnDestroy(): void {
    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }
  }
}
