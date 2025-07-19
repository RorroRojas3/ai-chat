import { Component, computed, signal } from '@angular/core';
import { StoreService } from '../../store/store.service';
import { ChatService } from '../../services/chat.service';
import { SessionService } from '../../services/session.service';
import markdownit from 'markdown-it';
import { DomSanitizer } from '@angular/platform-browser';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import { MessageDto } from '../../dtos/MessageDto';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [FormsModule],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent {
  constructor(
    public storeService: StoreService,
    private chatService: ChatService,
    private sessionService: SessionService,
    private sanitizer: DomSanitizer
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });

    // Set up debounced search
    this.searchSubject
      .pipe(debounceTime(600), distinctUntilChanged())
      .subscribe((query) => {
        this.performSearch(query);
      });
  }

  md: markdownit;
  private searchSubject = new Subject<string>();

  /**
   * Performs search using the session service
   */
  private performSearch(query: string): void {
    this.storeService.setSearchFilter(query);

    if (!query.trim()) {
      // If search is cleared, reload all sessions
      this.loadAllSessions();
      return;
    }

    this.storeService.setSearching(true);
    this.sessionService.searchSessions(query).subscribe({
      next: (sessions) => {
        this.storeService.sessions.set(sessions);
        this.storeService.setSearching(false);
      },
      error: () => {
        this.storeService.setSearching(false);
      },
    });
  }

  /**
   * Loads all sessions without search filter
   */
  private loadAllSessions(): void {
    this.sessionService.searchSessions('').subscribe((sessions) => {
      this.storeService.sessions.set(sessions);
    });
  }

  /**
   * Refreshes sessions after streaming is complete
   */
  refreshSessions(): void {
    if (this.storeService.hasSearchFilter()) {
      this.performSearch(this.storeService.searchFilter());
    } else {
      this.loadAllSessions();
    }
  }

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

  /**
   * Handles the click event for creating a new session.
   * Clears the current session data using the store service.
   *
   * @returns void
   */
  onClickCreateNewSession(): void {
    this.storeService.clearForNewSession();
  }

  /**
   * Handles search input changes
   */
  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }

  /**
   * Clears the search term
   */
  clearSearch(): void {
    this.storeService.clearSearchFilter();
    this.loadAllSessions();
  }
}
