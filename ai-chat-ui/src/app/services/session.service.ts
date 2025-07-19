import { Injectable } from '@angular/core';
import { SessionDto } from '../dtos/SessionDto';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';

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
   * Searches for sessions based on the provided query string.
   *
   * @param query - The search query string to filter sessions
   * @returns An Observable that emits an array of SessionDto objects matching the search criteria
   */
  searchSessions(query: string): Observable<SessionDto[]> {
    return this.http.get<SessionDto[]>(`${environment.apiUrl}sessions/search`, {
      params: { query },
    });
  }
}
