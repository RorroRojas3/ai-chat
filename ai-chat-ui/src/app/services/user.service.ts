import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);

  /**
   * Creates a new user by sending a POST request to the API.
   * @returns {Observable<void>} An Observable that completes when the user is successfully created.
   */
  createUser(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/users/me`, {});
  }
}
