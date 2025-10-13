import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { SessionDto } from '../../dtos/SessionDto';

@Component({
  selector: 'app-sessions',
  imports: [CommonModule, FormsModule],
  templateUrl: './sessions.component.html',
  styleUrl: './sessions.component.scss',
})
export class SessionsComponent implements OnInit {
  sessions = signal<SessionDto[]>([]);
  searchFilter = signal<string>('');
  isSearching = signal<boolean>(false);
  isLoadingMore = signal<boolean>(false);

  // Pagination
  currentSkip = signal<number>(0);
  take = signal<number>(10);
  hasMore = signal<boolean>(true);

  private searchTimeout: any;

  constructor(private sessionService: SessionService, private router: Router) {
    // Set up effect to watch for search filter changes
    effect(() => {
      const filter = this.searchFilter();

      // Clear any existing timeout
      if (this.searchTimeout) {
        clearTimeout(this.searchTimeout);
      }

      // Debounce the search by 600ms
      this.searchTimeout = setTimeout(() => {
        this.performSearch(filter);
      }, 600);
    });
  }

  ngOnInit(): void {
    this.loadSessions();
  }

  /**
   * Loads initial sessions
   */
  private loadSessions(): void {
    this.isSearching.set(true);
    this.sessionService
      .searchSessions('', this.currentSkip(), this.take())
      .subscribe({
        next: (sessions) => {
          this.sessions.set(sessions);
          this.hasMore.set(sessions.length === this.take());
          this.isSearching.set(false);
        },
        error: () => {
          this.isSearching.set(false);
        },
      });
  }

  /**
   * Performs search using the session service
   */
  private performSearch(query: string): void {
    this.currentSkip.set(0);
    this.isSearching.set(true);

    this.sessionService
      .searchSessions(query, this.currentSkip(), this.take())
      .subscribe({
        next: (sessions) => {
          this.sessions.set(sessions);
          this.hasMore.set(sessions.length === this.take());
          this.isSearching.set(false);
        },
        error: () => {
          this.isSearching.set(false);
        },
      });
  }

  /**
   * Handles search input changes
   */
  onSearchChange(value: string): void {
    this.searchFilter.set(value);
  }

  /**
   * Clears the search filter
   */
  clearSearch(): void {
    this.searchFilter.set('');
    this.currentSkip.set(0);
    this.loadSessions();
  }

  /**
   * Handles session click - navigates to the chat page with the session
   */
  onClickSession(sessionId: string): void {
    this.router.navigate(['/chat/session', sessionId]);
  }

  /**
   * Loads more sessions (pagination)
   */
  loadMore(): void {
    if (this.isLoadingMore() || !this.hasMore()) {
      return;
    }

    this.isLoadingMore.set(true);
    this.currentSkip.set(this.currentSkip() + this.take());

    this.sessionService
      .searchSessions(this.searchFilter(), this.currentSkip(), this.take())
      .subscribe({
        next: (sessions) => {
          this.sessions.set([...this.sessions(), ...sessions]);
          this.hasMore.set(sessions.length === this.take());
          this.isLoadingMore.set(false);
        },
        error: () => {
          this.isLoadingMore.set(false);
        },
      });
  }
}
