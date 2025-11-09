import { inject, Injectable } from '@angular/core';
import { SessionDto } from '../dtos/SessionDto';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';
import { DeactivateSessionBulkActionDto } from '../dtos/actions/session/DeactivateSessionBulkActionDto';
import { RenameSessionActionDto } from '../dtos/actions/session/RenameSessionActionDto';
import { CreateSessionActionDto } from '../dtos/actions/session/CreateSessionActionDto';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  private readonly http = inject(HttpClient);

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
   * Renames an existing session.
   *
   * @param request - The rename session request containing the session identifier and new name
   * @returns An Observable that completes when the session has been successfully renamed
   */
  renameSession(request: RenameSessionActionDto): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}sessions/rename`, request);
  }
}
