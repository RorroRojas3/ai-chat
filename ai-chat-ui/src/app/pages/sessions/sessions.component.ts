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
import { SessionRenameModalComponent } from '../../components/sessions/session-rename-modal/session-rename-modal.component';
import { RenameSessionActionDto } from '../../dtos/actions/session/RenameSessionActionDto';

@Component({
  selector: 'app-sessions',
  imports: [
    CommonModule,
    FormsModule,
    SessionDeleteModalComponent,
    SessionRenameModalComponent,
  ],
  templateUrl: './sessions.component.html',
  styleUrl: './sessions.component.scss',
})
export class SessionsComponent implements OnInit {
  // Constants
  readonly PAGE_SIZE = 10;
  readonly SEARCH_DEBOUNCE_MS = 600;
  readonly STORE_SYNC_LIMIT = 10;

  // Inject dependencies using Angular 19 pattern
  private readonly sessionService = inject(SessionService);
  private readonly router = inject(Router);
  private readonly storeService = inject(StoreService);
  private readonly destroyRef = inject(DestroyRef);

  sessions: SessionDto[] = [];
  searchFilter = '';
  isSearching = false;
  isLoadingMore = false;

  // Virtual scrolling properties
  currentSkip = 0;
  totalCount = 0;
  hasMoreSessions = true;
  showDeleteModal = false;
  selectedSessionIds: string[] = [];
  showRenameModal = false;
  sessionName = '';

  // Checkbox selection tracking
  hoveredSessionId: string | null = null;

  private searchSubject = new Subject<string>();

  constructor() {
    // Set up debounced search with switchMap to avoid race conditions
    this.searchSubject
      .pipe(
        debounceTime(this.SEARCH_DEBOUNCE_MS),
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
            this.PAGE_SIZE
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          this.handleSessionsResponse(response.items, response.totalCount);
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
   * Handles session response and updates component state
   */
  private handleSessionsResponse(
    items: SessionDto[],
    totalCount: number,
    append = false
  ): void {
    this.totalCount = totalCount;
    this.hasMoreSessions = this.currentSkip + this.PAGE_SIZE < totalCount;

    const mappedSessions = items.map((session) => new SessionDto(session));

    if (append) {
      this.sessions = [...this.sessions, ...mappedSessions];
    } else {
      this.sessions = mappedSessions;
    }
  }

  /**
   * Loads initial sessions
   */
  private loadSessions(): void {
    this.isSearching = true;
    this.currentSkip = 0;
    this.sessions = [];

    this.sessionService
      .searchSessions(this.searchFilter, this.currentSkip, this.PAGE_SIZE)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.handleSessionsResponse(response.items, response.totalCount);
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
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  /**
   * Clears the search term and reloads sessions
   */
  clearSearch(): void {
    this.searchFilter = '';
    this.loadSessions();
  }

  /**
   * TrackBy function for session list to improve performance
   */
  trackBySessionId(_index: number, session: SessionDto): string {
    return session.id;
  }

  /**
   * Loads more sessions (next page) and appends them to the existing list
   */
  loadMoreSessions(): void {
    if (!this.hasMoreSessions || this.isLoadingMore) {
      return;
    }

    this.isLoadingMore = true;
    this.currentSkip += this.PAGE_SIZE;

    this.sessionService
      .searchSessions(this.searchFilter, this.currentSkip, this.PAGE_SIZE)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.handleSessionsResponse(
            response.items,
            response.totalCount,
            true
          );
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
      this.closeDeleteModal();
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
            this.handleDeleteSuccess();
          },
          error: () => {
            this.closeDeleteModal();
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
            this.handleDeleteSuccess();
          },
          error: () => {
            this.closeDeleteModal();
          },
        });
    }
  }

  /**
   * Handles successful deletion by reloading sessions and updating store
   */
  private handleDeleteSuccess(): void {
    this.closeDeleteModal();
    this.loadSessions();
    this.updateStoreWithLatestSessions();
  }

  /**
   * Updates the store with the first page of sessions (no filters, no skip)
   */
  private updateStoreWithLatestSessions(): void {
    this.sessionService
      .searchSessions('', 0, this.STORE_SYNC_LIMIT)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          const latestSessions = response.items.map(
            (session) => new SessionDto(session)
          );
          this.storeService.updateSessions(latestSessions);
        },
      });
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedSessionIds = [];
  }

  onRenameSession(sessionId: string): void {
    const session = this.sessions.find((s) => s.id === sessionId);
    if (session) {
      this.selectedSessionIds = [sessionId];
      this.sessionName = session.name;
      this.showRenameModal = true;
    }
  }

  handleRenameSession(newName: string): void {
    if (this.selectedSessionIds.length !== 1) {
      this.closeRenameModal();
      return;
    }

    const sessionId = this.selectedSessionIds[0];
    const request = new RenameSessionActionDto(sessionId, newName);

    this.sessionService
      .renameSession(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.handleRenameSuccess();
        },
        error: () => {
          this.closeRenameModal();
        },
      });
  }

  /**
   * Handles successful rename by reloading sessions and updating store
   */
  private handleRenameSuccess(): void {
    this.closeRenameModal();
    this.loadSessions();
    this.updateStoreWithLatestSessions();
  }

  /**
   * Closes the rename modal and resets the session-related state.
   *
   * This method hides the rename modal dialog, clears the session name input,
   * and empties the array of selected session IDs.
   *
   * @returns {void}
   */
  closeRenameModal(): void {
    this.showRenameModal = false;
    this.sessionName = '';
    this.selectedSessionIds = [];
  }
}
