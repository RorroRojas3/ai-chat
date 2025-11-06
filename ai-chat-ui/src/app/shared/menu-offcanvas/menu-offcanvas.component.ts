import { Component } from '@angular/core';
import { StoreService } from '../../store/store.service';
import { SessionService } from '../../services/session.service';
import markdownit from 'markdown-it';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [FormsModule, CommonModule],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent {
  constructor(
    public storeService: StoreService,
    private sessionService: SessionService,
    private router: Router
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });

    // Set up debounced search
    this.searchSubject
      .pipe(debounceTime(600), distinctUntilChanged())
      .subscribe((filter) => {
        this.performSearch(filter);
      });
  }

  md: markdownit;
  private searchSubject = new Subject<string>();

  /**
   * Performs search using the session service
   */
  private performSearch(filter: string): void {
    this.storeService.setSearchFilter(filter);

    if (!filter.trim()) {
      // If search is cleared, reload all sessions
      this.loadAllSessions();
      return;
    }

    this.storeService.setSearching(true);
    this.sessionService.searchSessions(filter).subscribe({
      next: (response) => {
        this.storeService.sessions.set(response.items);
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
    this.sessionService.searchSessions('').subscribe((response) => {
      this.storeService.sessions.set(response.items);
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

  onClickSession(sessionId: string): void {
    this.storeService.sessionId.set(sessionId);
    this.router.navigate(['chat', 'session', sessionId]);
  }

  /**
   * Handles the click event for creating a new session.
   * Clears the current session data using the store service.
   *
   * @returns void
   */
  onClickCreateNewSession(): void {
    this.storeService.clearForNewSession();
    this.router.navigate(['chat']);
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

  /**
   * Navigates to the chat sessions page.
   *
   * @returns {void}
   */
  onClickGoToChats(): void {
    this.router.navigate(['sessions']);
  }
}
