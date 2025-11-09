import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { SessionDto } from '../../dtos/SessionDto';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  EMPTY,
  finalize,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StoreService } from '../../store/store.service';
import { SessionDeleteModalComponent } from '../../components/sessions/session-delete-modal/session-delete-modal.component';
import { DeactivateSessionBulkActionDto } from '../../dtos/actions/session/DeactivateSessionBulkActionDto';
import { SessionRenameModalComponent } from '../../components/sessions/session-rename-modal/session-rename-modal.component';
import { UpdateSessionActionDto } from '../../dtos/actions/session/UpdateSessionActionDto';
import { NotificationService } from '../../services/notification.service';

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
  readonly SEARCH_DEBOUNCE_MS = 600;
  readonly STORE_SYNC_LIMIT = 10;

  // Inject dependencies using Angular 19 pattern
  public readonly storeService = inject(StoreService);
  private readonly sessionService = inject(SessionService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);

  isLoadingMore = false;

  // Virtual scrolling properties
  showDeleteModal = false;
  selectedSessionIds: string[] = [];
  showRenameModal = false;
  sessionName = '';

  // Checkbox selection tracking
  hoveredSessionId: string | null = null;

  private searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.storeService.setPageSessionSearchFilter('');

    // Set up debounced search with switchMap to avoid race conditions
    this.searchSubject
      .pipe(
        debounceTime(this.SEARCH_DEBOUNCE_MS),
        distinctUntilChanged(),
        tap(() => {
          this.storeService.setPageSessionSearching(true);
          this.sessionService.clearPageSessions();
        }),
        switchMap((filter) => {
          this.sessionService.loadPageSessions(
            filter,
            0,
            this.storeService.SESSION_PAGE_SIZE
          );
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();

    // Load initial sessions
    this.sessionService.loadPageSessions(
      this.storeService.pageSessionSearchFilter(),
      0,
      this.storeService.SESSION_PAGE_SIZE
    );
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
    this.sessionService.loadPageSessions(
      '',
      0,
      this.storeService.SESSION_PAGE_SIZE
    );
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
    if (!this.storeService.pageSessionHasMore() || this.isLoadingMore) {
      return;
    }

    this.isLoadingMore = true;
    this.storeService.setPageSessionSkip(
      this.storeService.pageSessionSkip() + this.storeService.SESSION_PAGE_SIZE
    );

    this.sessionService
      .searchSessions(
        this.storeService.pageSessionSearchFilter(),
        this.storeService.pageSessionSkip(),
        this.storeService.SESSION_PAGE_SIZE
      )
      .pipe(
        catchError(() => {
          this.notificationService.error('Error loading chats.');
          return EMPTY;
        }),
        finalize(() => (this.isLoadingMore = false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((response) => {
        this.sessionService.handlePageSessionsResponse(
          response.items,
          response.totalCount,
          true
        );
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
        .pipe(
          catchError(() => {
            this.notificationService.error('Error deleting chat.');
            this.closeDeleteModal();
            return EMPTY;
          }),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe(() => {
          this.handleDeleteSuccess();
        });
    } else {
      // Multiple sessions, use bulk delete endpoint
      const bulkRequest = new DeactivateSessionBulkActionDto();
      bulkRequest.sessionIds = [...this.selectedSessionIds];

      this.sessionService
        .deactivateSessionBulk(bulkRequest)
        .pipe(
          catchError(() => {
            this.notificationService.error('Error deleting chats.');
            this.closeDeleteModal();
            return EMPTY;
          }),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe(() => {
          this.handleDeleteSuccess();
        });
    }
  }

  /**
   * Handles successful deletion by reloading sessions and updating store
   */
  private handleDeleteSuccess(): void {
    this.closeDeleteModal();
    this.sessionService.loadPageSessions(
      '',
      0,
      this.storeService.SESSION_PAGE_SIZE
    );
    this.sessionService.loadMenuSessions();
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedSessionIds = [];
  }

  onRenameSession(sessionId: string): void {
    const session = this.storeService
      .pageSessions()
      .find((s) => s.id === sessionId);
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
    const request = new UpdateSessionActionDto(sessionId, newName);

    this.sessionService
      .updateSession(request)
      .pipe(
        catchError(() => {
          this.notificationService.error('Error renaming chat.');
          this.closeRenameModal();
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.handleRenameSuccess();
      });
  }

  /**
   * Handles successful rename by reloading sessions and updating store
   */
  private handleRenameSuccess(): void {
    this.closeRenameModal();
    this.sessionService.loadPageSessions(
      '',
      0,
      this.storeService.SESSION_PAGE_SIZE
    );
    this.sessionService.loadMenuSessions();
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
