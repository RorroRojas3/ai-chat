import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StoreService } from '../../store/store.service';
import { SessionService } from '../../services/session.service';
import { FormsModule } from '@angular/forms';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  EMPTY,
  finalize,
  Subject,
} from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SessionDto } from '../../dtos/SessionDto';
import { SessionRenameModalComponent } from '../../components/sessions/session-rename-modal/session-rename-modal.component';
import { UpdateSessionActionDto } from '../../dtos/actions/session/UpdateSessionActionDto';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [FormsModule, CommonModule, SessionRenameModalComponent],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent implements OnInit {
  // Constants
  readonly MAX_SESSION_NAME_LENGTH = 40;
  readonly SEARCH_DEBOUNCE_MS = 600;

  // Inject dependencies using Angular 19 pattern
  public readonly storeService = inject(StoreService);
  private readonly sessionService = inject(SessionService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);

  private searchSubject = new Subject<string>();
  showRenameModal = false;

  ngOnInit(): void {
    // Set up debounced search with automatic cleanup
    this.searchSubject
      .pipe(
        debounceTime(this.SEARCH_DEBOUNCE_MS),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((filter: string) => {
        this.performSearch(filter);
      });
  }

  /**
   * Performs search using the session service
   */
  private performSearch(filter: string): void {
    this.storeService.setMenuSessionSearchFilter(filter);
    this.sessionService.loadMenuSessions();
  }

  /**
   * TrackBy function for session list to improve performance
   */
  trackBySessionId(_index: number, session: SessionDto): string {
    return session.id;
  }

  /**
   * Truncates session name if it exceeds max length
   */
  getTruncatedSessionName(session: SessionDto): string {
    return session.name.length > this.MAX_SESSION_NAME_LENGTH
      ? session.name.slice(0, this.MAX_SESSION_NAME_LENGTH) + '...'
      : session.name;
  }

  /**
   * Handles the click event for selecting a session from the menu.
   * Finds the session in the menu sessions list, sets it as the active session,
   * and navigates to the session's chat page.
   *
   * @param sessionId - The unique identifier of the session to navigate to
   * @returns void
   */
  onClickSession(sessionId: string): void {
    const foundSession = this.storeService
      .menuSessions()
      .find((s) => s.id === sessionId);
    if (foundSession) {
      this.storeService.session.set(foundSession);
      this.router.navigate(['chat', 'session', foundSession.id]);
    }
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
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  /**
   * Clears the search term
   */
  clearSearch(): void {
    this.storeService.clearMenuSessionSearchFilter();
    this.sessionService.loadMenuSessions();
  }

  /**
   * Navigates to the chat sessions page.
   *
   * @returns {void}
   */
  onClickGoToChats(): void {
    this.router.navigate(['sessions']);
  }

  /**
   * Navigates to the projects page.
   *
   * This method is typically called when the user clicks on a menu item
   * to navigate to the projects route.
   *
   * @returns {void}
   */
  onClickGoToProjects(): void {
    this.router.navigate(['projects']);
  }

  /**
   * Shows the rename modal dialog by setting the showRenameModal flag to true.
   * This method is typically called when the user initiates a rename action.
   */
  onShowRenameModal(): void {
    this.showRenameModal = true;
  }

  /**
   * Handles renaming a session with proper error handling and cleanup.
   * Updates the current session name, refreshes menu sessions, and optionally
   * reloads page sessions if the renamed session is present in the page view.
   *
   * @param newName - The new name for the session
   * @returns void
   *
   * @remarks
   * This method:
   * - Prevents memory leaks by using takeUntilDestroyed
   * - Shows error notifications if the rename operation fails
   * - Ensures the modal is closed in the finalize block regardless of success/failure
   * - Updates both the active session and menu sessions list
   * - Conditionally reloads page sessions if the session exists in that view
   */
  onRenameSession(newName: string): void {
    const currentSession = this.storeService.session();
    if (!currentSession) {
      return;
    }

    const request = new UpdateSessionActionDto(
      currentSession.id,
      newName,
      currentSession.projectId
    );

    this.sessionService
      .updateSession(request)
      .pipe(
        catchError(() => {
          this.notificationService.error(
            'Failed to rename session. Please try again.'
          );
          return EMPTY;
        }),
        finalize(() => this.onCloseRenameModal()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((response) => {
        this.storeService.session.set(response);
        this.sessionService.loadMenuSessions();

        if (
          this.storeService.pageSessions().some((s) => s.id === response.id)
        ) {
          this.sessionService.loadPageSessions(
            this.storeService.pageSessionSearchFilter(),
            0,
            this.storeService.SESSION_PAGE_SIZE
          );
        }
      });
  }

  /**
   * Closes the rename modal by setting the showRenameModal flag to false.
   * This method is typically called when the user cancels or completes the rename operation.
   */
  onCloseRenameModal(): void {
    this.showRenameModal = false;
  }
}
