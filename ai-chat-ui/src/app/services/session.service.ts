import { Injectable } from '@angular/core';
import { SessionDto } from '../dtos/SessionDto';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';
import { DeactivateSessionBulkActionDto } from '../dtos/actions/session/DeactivateSessionBulkActionDto';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  constructor(private http: HttpClient) {}

  /**
   * Creates a new session by sending a POST request to the sessions endpoint.
   *
   * @returns {Observable<SessionDto>} An observable that emits the created session data
   */
  createSession(): Observable<SessionDto> {
    return this.http.post<SessionDto>(`${environment.apiUrl}sessions`, {});
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
    return this.http.delete<void>(
      `${environment.apiUrl}sessions/${sessionId}/deactivate`
    );
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
}
