import { HttpClient } from '@angular/common/http';
import { inject, Injectable, NgZone } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionConversationDto } from '../dtos/SessionDto';
import { environment } from '../../environments/environment';
import { StoreService } from '../store/store.service';
import { MsalService } from '@azure/msal-angular';
import { CreateChatStreamActionDto } from '../dtos/actions/chats/CreateChatStreamActionDto';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly storeService = inject(StoreService);
  private readonly zone = inject(NgZone);
  private readonly msalService = inject(MsalService);

  /**
   * Creates an Observable that streams server-sent events from the chat API.
   *
   * This method establishes a streaming connection to the chat service endpoint,
   * sending a chat prompt and receiving real-time response data as it becomes available.
   * The stream is processed using the Fetch API with ReadableStream and decoded as text.
   *
   * @param prompt - The chat message or prompt to send to the AI service
   * @returns An Observable that emits string chunks of the streaming response
   *
   * @throws Will emit an error if the HTTP request fails or if there are network issues
   * @throws Will emit an error if the response body is null or cannot be read
   * @throws Will emit an error if authentication fails or token cannot be acquired
   */
  getServerSentEvent(prompt: string): Observable<string> {
    return new Observable((observer) => {
      let reader: ReadableStreamDefaultReader<Uint8Array> | undefined;
      const decoder = new TextDecoder();
      const abortController = new AbortController();

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
          // Don't emit error if it's an abort error
          if (error instanceof Error && error.name === 'AbortError') {
            this.zone.run(() => observer.complete());
          } else {
            this.zone.run(() => observer.error(error));
          }
        } finally {
          this.zone.run(() => observer.complete());
        }
      };

      // Acquire access token from MSAL
      const acquireToken = async () => {
        try {
          const account = this.msalService.instance.getActiveAccount();
          if (!account) {
            throw new Error('No active account! Please sign in.');
          }

          const tokenResponse =
            await this.msalService.instance.acquireTokenSilent({
              scopes: environment.apiConfig.scopes,
              account: account,
            });

          return tokenResponse.accessToken;
        } catch (error) {
          // If silent token acquisition fails, try interactive
          const tokenResponse =
            await this.msalService.instance.acquireTokenPopup({
              scopes: environment.apiConfig.scopes,
            });
          return tokenResponse.accessToken;
        }
      };

      // Start the fetch request with authentication
      acquireToken()
        .then((accessToken) => {
          return fetch(
            `${environment.apiUrl}chats/sessions/${
              this.storeService.session()?.id
            }/stream`,
            {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                Accept: 'text/event-stream',
                Authorization: `Bearer ${accessToken}`,
              },
              body: JSON.stringify(
                new CreateChatStreamActionDto(
                  prompt,
                  this.storeService.selectedModel().id,
                  this.storeService.selectedModel().aiServiceId,
                  this.storeService.selectedMcps()
                )
              ),
              signal: abortController.signal,
            }
          );
        })
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
          // Don't emit error if it's an abort error
          if (error instanceof Error && error.name === 'AbortError') {
            this.zone.run(() => observer.complete());
          } else {
            this.zone.run(() => observer.error(error));
          }
        });

      // Teardown logic: cancel the stream and abort the fetch request
      return () => {
        abortController.abort();
        reader?.cancel();
      };
    });
  }

  /**
   * Retrieves the conversation associated with the current session.
   *
   * @returns An Observable that emits a SessionConversationDto containing the conversation data
   * @throws HttpErrorResponse if the request fails or session is invalid
   */
  getSessionConversation(id: string): Observable<SessionConversationDto> {
    return this.http.get<SessionConversationDto>(
      `${environment.apiUrl}chats/sessions/${id}/conversations`
    );
  }
}
