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

@Component({
  selector: 'app-sessions',
  imports: [CommonModule, FormsModule],
  templateUrl: './sessions.component.html',
  styleUrl: './sessions.component.scss',
})
export class SessionsComponent implements OnInit {
  sessions: SessionDto[] = [];
  searchFilter: string = '';
  isSearching: boolean = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 0;
  totalCount: number = 0;

  private searchSubject = new Subject<string>();
  private destroyRef = inject(DestroyRef);

  constructor(private sessionService: SessionService, private router: Router) {
    // Set up debounced search with switchMap to avoid race conditions
    this.searchSubject
      .pipe(
        debounceTime(600),
        distinctUntilChanged(),
        tap(() => {
          this.isSearching = true;
          this.currentPage = 1; // Reset to first page on new search
        }),
        switchMap((filter) => {
          this.searchFilter = filter;
          const skip = (this.currentPage - 1) * this.pageSize;
          return this.sessionService.searchSessions(
            filter,
            skip,
            this.pageSize
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          // Update pagination metadata
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;

          // Map plain objects to SessionDto instances
          this.sessions = response.items.map((session) => {
            const dto = new SessionDto();
            dto.id = session.id;
            dto.name = session.name;
            dto.dateCreated = session.dateCreated;
            dto.dateModified = session.dateModified;
            return dto;
          });
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
   * Loads sessions with pagination
   */
  private loadSessions(): void {
    const skip = (this.currentPage - 1) * this.pageSize;
    this.isSearching = true;

    this.sessionService
      .searchSessions(this.searchFilter, skip, this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          // Update pagination metadata
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;

          // Map plain objects to SessionDto instances
          this.sessions = response.items.map((session) => {
            const dto = new SessionDto();
            dto.id = session.id;
            dto.name = session.name;
            dto.dateCreated = session.dateCreated;
            dto.dateModified = session.dateModified;
            return dto;
          });
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
   * Clears the search term
   */
  clearSearch(): void {
    this.searchFilter = '';
    this.currentPage = 1;
    this.loadSessions();
  }

  /**
   * Navigates to the selected session
   */
  onClickSession(sessionId: string): void {
    this.router.navigate(['chat', 'session', sessionId]);
  }

  /**
   * Navigates to the next page
   */
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadSessions();
    }
  }

  /**
   * Navigates to the previous page
   */
  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadSessions();
    }
  }

  /**
   * Navigates to the first page
   */
  firstPage(): void {
    if (this.currentPage !== 1) {
      this.currentPage = 1;
      this.loadSessions();
    }
  }

  /**
   * Navigates to the last page
   */
  lastPage(): void {
    if (this.currentPage !== this.totalPages) {
      this.currentPage = this.totalPages;
      this.loadSessions();
    }
  }

  /**
   * Goes to a specific page
   */
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadSessions();
    }
  }

  /**
   * Returns whether there are more pages available
   */
  get hasMoreResults(): boolean {
    return this.currentPage < this.totalPages;
  }

  /**
   * Returns whether there are previous pages available
   */
  get hasPreviousResults(): boolean {
    return this.currentPage > 1;
  }

  /**
   * Generates an array of page numbers to display in pagination
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    const halfRange = Math.floor(maxPagesToShow / 2);

    let startPage = Math.max(1, this.currentPage - halfRange);
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    // Adjust start page if we're near the end
    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }
}
