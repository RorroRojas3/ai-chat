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
import { DeactivateSessionBulkActionDto } from '../../dtos/actions/session/DeactivateSessionBulkActionDto';

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
  showDeleteModal: boolean = false;
  selectedSessionIds: string[] = [];

  // Checkbox selection tracking
  hoveredSessionId: string | null = null;

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

  /**
   * Toggles the selection state of a session checkbox
   */
  toggleSessionSelection(sessionId: string, event: Event): void {
    event.stopPropagation(); // Prevent navigation when clicking checkbox

    const index = this.selectedSessionIds.indexOf(sessionId);
    if (index > -1) {
      this.selectedSessionIds.splice(index, 1);
    } else {
      this.selectedSessionIds.push(sessionId);
    }
  }

  /**
   * Checks if a session is selected
   */
  isSessionSelected(sessionId: string): boolean {
    return this.selectedSessionIds.includes(sessionId);
  }

  /**
   * Checks if checkbox should be visible for a session
   */
  shouldShowCheckbox(sessionId: string): boolean {
    return (
      this.hoveredSessionId === sessionId || this.selectedSessionIds.length > 0
    );
  }

  /**
   * Handles mouse enter on a session item
   */
  onSessionMouseEnter(sessionId: string): void {
    this.hoveredSessionId = sessionId;
  }

  /**
   * Handles mouse leave on a session item
   */
  onSessionMouseLeave(): void {
    this.hoveredSessionId = null;
  }

  /**
   * Deselects all selected sessions
   */
  deselectAllSessions(): void {
    this.selectedSessionIds = [];
  }

  /**
   * Initiates deletion of selected sessions
   */
  onDeleteSelectedSessions(): void {
    if (this.selectedSessionIds.length === 0) {
      return;
    }
    this.showDeleteModal = true;
  }

  onDeleteSession(sessionId: string): void {
    this.selectedSessionIds = [sessionId];
    this.showDeleteModal = true;
  }

  handleDeleteSession(): void {
    if (this.selectedSessionIds.length === 0) {
      this.showDeleteModal = false;
      return;
    }

    // If single session, use single delete endpoint
    if (this.selectedSessionIds.length === 1) {
      const sessionId = this.selectedSessionIds[0];
      this.sessionService
        .deactivateSession(sessionId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.showDeleteModal = false;
            this.selectedSessionIds = [];
            // Reload sessions to sync with server
            this.loadSessions();
            // Update store with first 10 sessions (no filters, no skip)
            this.updateStoreWithLatestSessions();
          },
          error: (error) => {
            console.error('Error deleting session:', error);
            this.showDeleteModal = false;
            this.selectedSessionIds = [];
          },
        });
    } else {
      // Multiple sessions, use bulk delete endpoint
      const bulkRequest = new DeactivateSessionBulkActionDto();
      bulkRequest.sessionIds = [...this.selectedSessionIds];

      this.sessionService
        .deactivateSessionBulk(bulkRequest)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.showDeleteModal = false;
            this.selectedSessionIds = [];
            // Reload sessions to sync with server
            this.loadSessions();
            // Update store with first 10 sessions (no filters, no skip)
            this.updateStoreWithLatestSessions();
          },
          error: (error) => {
            console.error('Error deleting sessions:', error);
            this.showDeleteModal = false;
            this.selectedSessionIds = [];
          },
        });
    }
  }

  /**
   * Updates the store with the first 10 sessions (no filters, no skip)
   */
  private updateStoreWithLatestSessions(): void {
    this.sessionService
      .searchSessions('', 0, 10)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          const latestSessions = response.items.map(
            (session) => new SessionDto(session)
          );
          this.storeService.updateSessions(latestSessions);
        },
        error: (error) => {
          console.error('Error updating store sessions:', error);
        },
      });
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedSessionIds = [];
  }
}
