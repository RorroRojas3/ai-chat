import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, Subject, Subscription, takeUntil } from 'rxjs';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

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
    });
  }

  prompt: string = '';
  message?: string = '';
  md: markdownit;
  sseSubscription: Subscription | undefined;
  private destroy$ = new Subject<void>();
  @Output() markdown = new EventEmitter<SafeHtml>();

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

      // this.sseSubscription = this.chatService
      //   .getServerSentEvent(this.prompt)
      //   .subscribe((message) => {
      //     this.storeService.stream.update((stream) => stream + message);
      //   });

      this.sseSubscription = this.chatService
        .getServerSentEvent(this.prompt)
        .subscribe((message) => {
          this.storeService.stream.update((stream) => stream + message);
          const html = this.md.render(this.storeService.stream());
          const sanitzeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
          this.markdown.emit(sanitzeHtml);
        });

      // this.chatService
      //   .createStreamMessage(this.prompt)
      //   .pipe(takeUntil(this.destroy$))
      //   .subscribe({
      //     next: (data) => {
      //       this.storeService.stream.set(this.storeService.stream() + data);
      //     },
      //     error: (error) => {
      //       console.error('Stream error:', error);
      //     },
      //   });

      // const response = await firstValueFrom(
      //   this.chatService.createCompleteMessage(this.prompt)
      // );
      // this.message = response.message;
      // const html = this.md.render(this.message ?? '');
      // const sanitzeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
      // this.markdown.emit(sanitzeHtml);

      this.prompt = '';
    } catch (error) {
      // Handle errors appropriately
      console.error('Error in session creation:', error);
      // You might want to show an error message to the user
    } finally {
      // Re-enable the button regardless of success/failure
      this.storeService.disablePromptButton.set(false);
    }
  }

  ngOnDestroy(): void {
    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }
  }
}
