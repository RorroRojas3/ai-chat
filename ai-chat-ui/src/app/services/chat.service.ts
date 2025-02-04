import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionDto } from '../dtos/SessionDto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  constructor(private http: HttpClient) {}

  createSession(): Observable<SessionDto> {
    return this.http.post<SessionDto>(`${environment.apiUrl}chat/session`, {});
  }
}
