import { Component } from '@angular/core';
import { StoreService } from '../../store/store.service';
import { ChatService } from '../../services/chat.service';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import { MessageDto } from '../../dtos/MessageDto';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent {
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

  md: markdownit;

  /**
   * Handles session change by updating the current session and retrieving its conversation history.
   * Processes the conversation messages, converting assistant messages from markdown to sanitized HTML.
   * Updates the store with the processed messages.
   *
   * @param sessionId - The unique identifier of the selected session
   * @returns void
   */
  onClickSession(sessionId: string): void {
    this.storeService.sessionId.set(sessionId);
    this.storeService.disablePromptButton.set(true);
    this.chatService.getSessionConversation().subscribe((response) => {
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
    });
  }

  onClickCreateNewSession(): void {
    this.storeService.clearForNewSession();
  }
}
