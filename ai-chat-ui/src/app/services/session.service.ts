import { inject, Injectable } from '@angular/core';
import { SessionDto } from '../dtos/SessionDto';
import { catchError, EMPTY, finalize, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';
import { DeactivateSessionBulkActionDto } from '../dtos/actions/session/DeactivateSessionBulkActionDto';
import { UpdateSessionActionDto } from '../dtos/actions/session/UpdateSessionActionDto';
import { CreateSessionActionDto } from '../dtos/actions/session/CreateSessionActionDto';
import { StoreService } from '../store/store.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  private readonly http = inject(HttpClient);
  private readonly storeService = inject(StoreService);
  private readonly notificationService = inject(NotificationService);

  /**
   * Retrieves a specific session by its unique identifier.
   *
   * @param {string} sessionId - The unique identifier of the session to retrieve
   * @returns {Observable<SessionDto>} An Observable that emits the SessionDto for the requested session
   */
  getSession(sessionId: string): Observable<SessionDto> {
    return this.http.get<SessionDto>(
      `${environment.apiUrl}sessions/${sessionId}`
    );
  }

  /**
   * Creates a new session by sending a POST request to the sessions endpoint.
   *
   * @param request - The data transfer object containing the information needed to create a session
   * @returns An Observable that emits the created SessionDto upon successful completion
   */
  createSession(request: CreateSessionActionDto): Observable<SessionDto> {
    return this.http.post<SessionDto>(`${environment.apiUrl}sessions`, request);
  }

  /**
   * Searches for sessions based on a filter string with pagination support.
   *
   * @param filter - The search filter string to match against sessions
   * @param skip - The number of records to skip for pagination (default: 0)
   * @param take - The number of records to retrieve (default: 10)
   * @returns An Observable that emits an array of SessionDto objects matching the search criteria
   */
  searchSessions(
    filter: string,
    skip: number = 0,
    take: number = 10
  ): Observable<PaginatedResponseDto<SessionDto>> {
    return this.http.get<PaginatedResponseDto<SessionDto>>(
      `${environment.apiUrl}sessions/search`,
      {
        params: { filter, skip, take },
      }
    );
  }

  /**
   * Deactivates a session by sending a DELETE request to the API.
   *
   * @param sessionId - The unique identifier of the session to deactivate
   * @returns An Observable that completes when the session is successfully deactivated
   *
   * @remarks
   * This method sends a DELETE request to the `/sessions/{sessionId}/deactivate` endpoint.
   * The operation completes without returning any data upon success.
   */
  deactivateSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}sessions/${sessionId}`);
  }

  /**
   * Deactivates multiple sessions in bulk.
   *
   * @param request - The bulk deactivation request containing session identifiers to deactivate
   * @returns An Observable that completes when the sessions have been deactivated
   *
   * @remarks
   * This method sends a DELETE request to the sessions bulk endpoint with the deactivation
   * criteria in the request body.
   */
  deactivateSessionBulk(
    request: DeactivateSessionBulkActionDto
  ): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}sessions/bulk`, {
      body: request,
    });
  }

  /**
   * Updates an existing session by sending a PUT request to the sessions endpoint.
   *
   * @param request - The update session request containing the session identifier and updated properties (e.g., name)
   * @returns An Observable that emits the updated SessionDto upon successful completion
   *
   * @remarks
   * This method sends a PUT request to the `/sessions` endpoint with the session update data.
   * The updated session data is returned in the response.
   */
  updateSession(request: UpdateSessionActionDto): Observable<SessionDto> {
    return this.http.put<SessionDto>(`${environment.apiUrl}sessions`, request);
  }

  /**
   * Loads and updates the menu sessions in the store.
   *
   * Fetches sessions based on the current menu search filter and updates the store
   * with the results. Shows an error notification if the request fails.
   * Sets the menu session searching state while the request is in progress.
   */
  loadMenuSessions(): void {
    this.storeService.setMenuSessionSearching(true);
    this.searchSessions(
      this.storeService.menuSessionSearchFilter(),
      0,
      this.storeService.SESSION_PAGE_SIZE
    )
      .pipe(
        catchError(() => {
          this.notificationService.error('Error loading chats.');
          return EMPTY;
        }),
        finalize(() => this.storeService.setMenuSessionSearching(false))
      )
      .subscribe((response) => {
        this.storeService.updateMenuSessions(response.items);
      });
  }

  /**
   * Loads page sessions with the specified filter and pagination parameters.
   *
   * Updates the store with the search filter, skip offset, and total count, then fetches
   * sessions from the API. Shows an error notification if the request fails.
   * Sets the page session searching state while the request is in progress.
   *
   * @param filter - The search filter string to match against sessions
   * @param skip - The number of records to skip for pagination (default: 0)
   * @param take - The number of records to retrieve (default: 10)
   */
  loadPageSessions(filter: string, skip: number = 0, take: number = 10): void {
    this.storeService.setPageSessionSearching(true);
    this.storeService.setPageSessionSearchFilter(filter);
    this.storeService.setPageSessionSkip(skip);
    this.searchSessions(filter, skip, take)
      .pipe(
        catchError(() => {
          this.notificationService.error('Error loading chats.');
          return EMPTY;
        }),
        finalize(() => this.storeService.setPageSessionSearching(false))
      )
      .subscribe((response) => {
        this.handlePageSessionsResponse(response.items, response.totalCount);
      });
  }

  /**
   * Clears all page sessions data from the store.
   *
   * Resets the page sessions array to empty, resets the skip offset to 0,
   * and sets the total count to 0.
   */
  clearPageSessions(): void {
    this.storeService.updatePageSessions([]);
    this.storeService.setPageSessionSkip(0);
    this.storeService.setPageSessionTotalCount(0);
  }

  /**
   * Handles the response from the page sessions API and updates the store accordingly.
   *
   * @param items - Array of session DTOs returned from the API
   * @param totalCount - Total number of sessions available
   * @param append - If true, appends items to existing sessions; if false, replaces them. Defaults to false
   *
   * @remarks
   * This method performs the following operations:
   * - Updates the total count of page sessions in the store
   * - Calculates and sets whether more sessions are available for pagination
   * - Either appends the new items to existing sessions or replaces them entirely based on the append flag
   */
  handlePageSessionsResponse(
    items: SessionDto[],
    totalCount: number,
    append = false
  ): void {
    this.storeService.setPageSessionTotalCount(totalCount);
    this.storeService.setPageSessionHasMore(
      this.storeService.pageSessionSkip() +
        this.storeService.SESSION_PAGE_SIZE <
        totalCount
    );

    // API already returns SessionDto-shaped objects
    if (append) {
      this.storeService.updatePageSessions([
        ...this.storeService.pageSessions(),
        ...items,
      ]);
    } else {
      this.storeService.updatePageSessions(items);
    }
  }
}
