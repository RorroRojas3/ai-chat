import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { SessionDto } from '../../dtos/SessionDto';
import {
  debounceTime,
  distinctUntilChanged,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StoreService } from '../../store/store.service';
import { SessionDeleteModalComponent } from '../../components/sessions/session-delete-modal/session-delete-modal.component';

@Component({
  selector: 'app-sessions',
  imports: [CommonModule, FormsModule, SessionDeleteModalComponent],
  templateUrl: './sessions.component.html',
  styleUrl: './sessions.component.scss',
})
export class SessionsComponent implements OnInit {
  sessions: SessionDto[] = [];
  searchFilter: string = '';
  isSearching: boolean = false;
  isLoadingMore: boolean = false;

  // Virtual scrolling properties
  pageSize: number = 10;
  currentSkip: number = 0;
  totalCount: number = 0;
  hasMoreSessions: boolean = true;
  showDeleteModal: boolean = true;

  private searchSubject = new Subject<string>();
  private destroyRef = inject(DestroyRef);

  constructor(
    private sessionService: SessionService,
    private router: Router,
    private storeService: StoreService
  ) {
    // Set up debounced search with switchMap to avoid race conditions
    this.searchSubject
      .pipe(
        debounceTime(600),
        distinctUntilChanged(),
        tap(() => {
          this.isSearching = true;
          this.currentSkip = 0; // Reset to beginning on new search
          this.sessions = []; // Clear existing sessions on new search
        }),
        switchMap((filter) => {
          this.searchFilter = filter;
          return this.sessionService.searchSessions(
            filter,
            this.currentSkip,
            this.pageSize
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          // Update metadata
          this.totalCount = response.totalCount;
          this.hasMoreSessions =
            this.currentSkip + this.pageSize < response.totalCount;

          // Map plain objects to SessionDto instances
          this.sessions = response.items.map(
            (session) => new SessionDto(session)
          );
          this.isSearching = false;
        },
        error: () => {
          this.isSearching = false;
        },
      });
  }

  ngOnInit(): void {
    this.loadSessions();
  }

  /**
   * Loads initial sessions
   */
  private loadSessions(): void {
    this.isSearching = true;
    this.currentSkip = 0;
    this.sessions = [];

    this.sessionService
      .searchSessions(this.searchFilter, this.currentSkip, this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          // Update metadata
          this.totalCount = response.totalCount;
          this.hasMoreSessions =
            this.currentSkip + this.pageSize < response.totalCount;

          // Map plain objects to SessionDto instances
          this.sessions = response.items.map(
            (session) => new SessionDto(session)
          );
          this.isSearching = false;
        },
        error: () => {
          this.isSearching = false;
        },
      });
  }

  /**
   * Handles search input changes
   */
  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }

  /**
   * Clears the search term and reloads sessions
   */
  clearSearch(): void {
    this.searchFilter = '';
    this.loadSessions();
  }

  /**
   * Loads more sessions (next 10) and appends them to the existing list
   */
  loadMoreSessions(): void {
    if (!this.hasMoreSessions || this.isLoadingMore) {
      return;
    }

    this.isLoadingMore = true;
    this.currentSkip += this.pageSize;

    this.sessionService
      .searchSessions(this.searchFilter, this.currentSkip, this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          // Update metadata
          this.totalCount = response.totalCount;
          this.hasMoreSessions =
            this.currentSkip + this.pageSize < response.totalCount;

          // Append new sessions to the existing list
          const newSessions = response.items.map(
            (session) => new SessionDto(session)
          );
          this.sessions = [...this.sessions, ...newSessions];
          this.isLoadingMore = false;
        },
        error: () => {
          this.isLoadingMore = false;
        },
      });
  }

  /**
   * Navigates to the selected session
   */
  onClickSession(sessionId: string): void {
    this.router.navigate(['chat', 'session', sessionId]);
  }

  onClickCreateNewSession(): void {
    this.storeService.clearForNewSession();
    this.router.navigate(['chat']);
  }

  handleDeleteSession(): void {
    // Implement the logic to delete the session here
    this.showDeleteModal = false;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }
}
