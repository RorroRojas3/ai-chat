import { Component, computed, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';
import { StoreService } from '../../store/store.service';
import { MessageBubbleComponent } from '../../components/home/message-bubble/message-bubble.component';
import { MessageDto } from '../../dtos/MessageDto';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import hljs from 'highlight.js';
import { ActivatedRoute } from '@angular/router';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-home',
  imports: [PromptBoxComponent, MessageBubbleComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  // Inject dependencies using Angular 19 pattern
  public readonly storeService = inject(StoreService);
  private readonly chatService = inject(ChatService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  private readonly md: markdownit;

  canHighlight = computed(() => {
    return this.storeService.isStreaming();
  });

  constructor() {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });
  }

  ngOnInit(): void {
    // Listen to route parameter changes to load conversation when sessionId changes
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const sessionId = params['sessionId'];
        if (sessionId) {
          this.loadSessionConversation(sessionId);
        }
      });
  }

  /**
   * Loads the conversation for a given session ID.
   * Processes the conversation messages, converting assistant messages from markdown to sanitized HTML.
   * Updates the store with the processed messages.
   * If the API call fails (e.g., invalid sessionId), resets to a new session.
   *
   * @param sessionId - The unique identifier of the session to load
   */
  private loadSessionConversation(sessionId: string): void {
    this.storeService.sessionId.set(sessionId);
    this.storeService.disablePromptButton.set(true);
    this.chatService.getSessionConversation().subscribe({
      next: (response) => {
        const mappedMessages = response.messages.map((message) => {
          if (message.role === 1) {
            const html = this.md.render(message.text);
            const sanitizeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
            return new MessageDto('', false, sanitizeHtml);
          }
          return new MessageDto(message.text, true, undefined);
        });
        this.storeService.messages.set(mappedMessages);
        this.storeService.disablePromptButton.set(false);
      },
      error: () => {
        // If session is invalid or API call fails, reset to a new session
        this.storeService.clearForNewSession();
      },
    });
  }

  /**
   * Handles changes to the markdown content by synchronizing the change
   * with the stream message in the store service.
   *
   * @param markdown - The updated markdown content as SafeHtml
   */
  onMarkdownChange(markdown: SafeHtml): void {
    this.storeService.streamMessage.update((streamMessage) => ({
      ...streamMessage,
      markdown,
    }));
  }

  /**
   * TrackBy function for message list to improve performance
   */
  trackByMessage(index: number, _message: MessageDto): number {
    return index;
  }
}
