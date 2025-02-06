import { HttpClient } from '@angular/common/http';
import { Injectable, NgZone } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionDto } from '../dtos/SessionDto';
import { environment } from '../../environments/environment';
import { ChatCompletionDto } from '../dtos/ChatCompletionDto';
import { StoreService } from '../store/store.service';
import { ChatCompletionRequestDto } from '../dtos/ChatCompletionRequestDto';
import { ChatStreamRequestDto } from '../dtos/ChatStreamRequestDto';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  constructor(
    private http: HttpClient,
    private storeService: StoreService,
    private zone: NgZone
  ) {}

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

  createStreamMessage(prompt: string): Observable<string> {
    return this.http.post(
      `${
        environment.apiUrl
      }chat/session/${this.storeService.sessionId()}/stream`,
      new ChatStreamRequestDto(prompt),
      {
        responseType: 'text',
        observe: 'body',
        headers: {
          Accept: 'text/event-stream',
        },
      }
    );
  }

  getServerSentEvent(prompt: string): Observable<string> {
    return new Observable((observer) => {
      let reader: ReadableStreamDefaultReader<Uint8Array> | undefined;
      const decoder = new TextDecoder();

      const readStream = async (
        streamReader: ReadableStreamDefaultReader<Uint8Array>
      ) => {
        try {
          while (true) {
            const { value, done } = await streamReader.read();
            if (done) break;

            const text = decoder.decode(value, { stream: true });
            this.zone.run(() => observer.next(text));
          }
        } catch (error) {
          this.zone.run(() => observer.error(error));
        } finally {
          this.zone.run(() => observer.complete());
        }
      };

      fetch(
        `${
          environment.apiUrl
        }chat/session/${this.storeService.sessionId()}/stream`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Accept: 'text/event-stream',
          },
          body: JSON.stringify(new ChatStreamRequestDto(prompt)),
        }
      )
        .then((response) => {
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
          if (!response.body) {
            throw new Error('Response body is null');
          }
          reader = response.body.getReader();
          return readStream(reader);
        })
        .catch((error) => {
          this.zone.run(() => observer.error(error));
        });

      return () => {
        reader?.cancel();
      };
    });
  }
}
