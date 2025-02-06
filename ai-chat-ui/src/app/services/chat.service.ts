import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionDto } from '../dtos/SessionDto';
import { environment } from '../../environments/environment';
import { ChatCompletionDto } from '../dtos/ChatCompletionDto';
import { StoreService } from '../store/store.service';
import { ChatCompletionRequestDto } from '../dtos/ChatCompletionRequestDto';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  constructor(private http: HttpClient, private storeService: StoreService) {}

  createSession(): Observable<SessionDto> {
    return this.http.post<SessionDto>(`${environment.apiUrl}chat/session`, {});
  }

  createCompleteMessage(prompt: string): Observable<ChatCompletionDto> {
    return this.http.post<ChatCompletionDto>(
      `${
        environment.apiUrl
      }chat/session/${this.storeService.sessionId()}/completion`,
      new ChatCompletionRequestDto(prompt)
    );
  }
}
