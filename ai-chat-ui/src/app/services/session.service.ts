import { Injectable } from '@angular/core';
import { SessionDto } from '../dtos/SessionDto';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';

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
}
