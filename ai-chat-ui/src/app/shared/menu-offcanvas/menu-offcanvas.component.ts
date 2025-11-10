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
  forkJoin,
  of,
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
    this.sessionService.loadMenuSessions().subscribe();
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
   * Checks if the given session ID matches the current active session
   */
  isCurrentSession(sessionId: string): boolean {
    return this.storeService.session()?.id === sessionId;
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
    this.sessionService.loadMenuSessions().subscribe();
  }

  /**
   * Navigates to the chat sessions page.
   *
   * @returns {void}
   */
  onClickGoToChats(): void {
    this.storeService.clearForNewSession();
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
    this.storeService.clearForNewSession();
    this.router.navigate(['projects']);
  }

  /**
   * Initiates the rename process for a specific session.
   *
   * Finds the session by ID, sets it as the active session in the store,
   * closes the offcanvas menu, and opens the rename modal.
   *
   * @param sessionId - The unique identifier of the session to rename
   * @param event - The click event to stop propagation
   * @returns void
   */
  onRenameSession(sessionId: string, event: Event): void {
    event.stopPropagation();

    const session = this.storeService
      .menuSessions()
      .find((s) => s.id === sessionId);
    if (session) {
      this.storeService.session.set(session);

      // Close the offcanvas before opening the modal
      this.toggleOffcanvas(false);

      // Small delay to ensure offcanvas closes before modal opens
      setTimeout(() => {
        this.showRenameModal = true;
      }, 300);
    }
  }

  /**
   * Handles renaming the active session with proper error handling and cleanup.
   * Updates the session name, refreshes menu sessions, and optionally
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
   * - Reopens the offcanvas menu after the operation completes
   * - Updates the active session in the store
   * - Reloads menu sessions to reflect the change
   * - Conditionally reloads page sessions if the session exists in that view
   */
  handleRenameSession(newName: string): void {
    const currentSession = this.storeService.session();
    if (!currentSession) {
      this.onCloseRenameModal();
      return;
    }

    const request = new UpdateSessionActionDto(currentSession.id, newName);

    this.sessionService
      .updateSession(request)
      .pipe(
        catchError(() => {
          this.notificationService.error('Error renaming chat.');
          return EMPTY;
        }),
        finalize(() => {
          this.onCloseRenameModal();
          this.toggleOffcanvas(true);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((response) => {
        // Update the active session with the response
        this.storeService.session.set(response);

        // Check if we need to reload page sessions
        const shouldReloadPageSessions = this.storeService
          .pageSessions()
          .some((s) => s.id === response.id);

        // Reload menu sessions and conditionally reload page sessions
        forkJoin({
          menuSessions: this.sessionService.loadMenuSessions(),
          pageSessions: shouldReloadPageSessions
            ? this.sessionService.loadPageSessions(
                this.storeService.pageSessionSearchFilter(),
                0,
                this.storeService.SESSION_PAGE_SIZE
              )
            : of(undefined),
        }).subscribe();
      });
  }

  /**
   * Closes the rename modal and resets the modal state.
   *
   * @returns void
   */
  onCloseRenameModal(): void {
    this.showRenameModal = false;
  }

  /**
   * Programmatically toggles the offcanvas menu open or closed.
   * Uses Bootstrap's offcanvas API to show or hide the menu.
   *
   * @param open - True to open the offcanvas, false to close it
   * @returns void
   */
  private toggleOffcanvas(open: boolean): void {
    const offcanvasElement = document.getElementById('menu-offcanvas');
    if (offcanvasElement) {
      // Get or create Bootstrap's Offcanvas instance
      let bsOffcanvas = (window as any).bootstrap?.Offcanvas?.getInstance(
        offcanvasElement
      );
      if (!bsOffcanvas) {
        bsOffcanvas = new (window as any).bootstrap.Offcanvas(offcanvasElement);
      }

      if (open) {
        bsOffcanvas.show();
      } else {
        bsOffcanvas.hide();
      }
    }
  }
}
