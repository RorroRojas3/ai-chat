import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StoreService } from '../../store/store.service';
import { SessionService } from '../../services/session.service';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SessionDto } from '../../dtos/SessionDto';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [FormsModule, CommonModule],
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

  private searchSubject = new Subject<string>();

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
    this.sessionService.searchSessions('').subscribe({
      next: (response) => {
        this.storeService.sessions.set(response.items);
      },
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
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
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
}
