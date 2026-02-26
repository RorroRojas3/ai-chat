import { HttpClient } from '@angular/common/http';
import { inject, Injectable, NgZone } from '@angular/core';
import { catchError, EMPTY, finalize, map, Observable, tap } from 'rxjs';
import { SessionConversationDto, SessionDto } from '../dtos/SessionDto';
import { environment } from '../../environments/environment';
import { StoreService } from '../store/store.service';
import { ModelStore } from '../store/model.store';
import { McpStore } from '../store/mcp.store';
import { MsalService } from '@azure/msal-angular';
import { CreateChatStreamActionDto } from '../dtos/actions/chats/CreateChatStreamActionDto';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';
import { DeactivateSessionBulkActionDto } from '../dtos/actions/session/DeactivateSessionBulkActionDto';
import { UpdateSessionActionDto } from '../dtos/actions/session/UpdateSessionActionDto';
import { CreateSessionActionDto } from '../dtos/actions/session/CreateSessionActionDto';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  private readonly http = inject(HttpClient);
  private readonly storeService = inject(StoreService);
  private readonly modelStore = inject(ModelStore);
  private readonly mcpStore = inject(McpStore);
  private readonly zone = inject(NgZone);
  private readonly msalService = inject(MsalService);
  private readonly notificationService = inject(NotificationService);

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
        streamReader: ReadableStreamDefaultReader<Uint8Array>,
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
            `${environment.apiUrl}conversations/${
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
                  this.modelStore.selectedModel()!.id,
                  this.modelStore.selectedModel()!.aiServiceId,
                  this.mcpStore.selectedMcps(),
                ),
              ),
              signal: abortController.signal,
            },
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
      `${environment.apiUrl}conversations/${id}/messages`,
    );
  }

  /**
   * Retrieves a specific session by its unique identifier.
   *
   * @param {string} sessionId - The unique identifier of the session to retrieve
   * @returns {Observable<SessionDto>} An Observable that emits the SessionDto for the requested session
   */
  getConversation(sessionId: string): Observable<SessionDto> {
    return this.http.get<SessionDto>(
      `${environment.apiUrl}conversations/${sessionId}`,
    );
  }

  /**
   * Creates a new session by sending a POST request to the sessions endpoint.
   *
   * @param request - The data transfer object containing the information needed to create a session
   * @returns An Observable that emits the created SessionDto upon successful completion
   */
  createConversation(request: CreateSessionActionDto): Observable<SessionDto> {
    return this.http.post<SessionDto>(
      `${environment.apiUrl}conversations`,
      request,
    );
  }

  /**
   * Searches for sessions based on a filter string with pagination support.
   *
   * @param filter - The search filter string to match against sessions
   * @param skip - The number of records to skip for pagination (default: 0)
   * @param take - The number of records to retrieve (default: 10)
   * @returns An Observable that emits a paginated response of SessionDto objects matching the search criteria
   */
  searchConversations(
    filter: string,
    skip: number = 0,
    take: number = 10,
  ): Observable<PaginatedResponseDto<SessionDto>> {
    return this.http.get<PaginatedResponseDto<SessionDto>>(
      `${environment.apiUrl}conversations/search`,
      {
        params: { filter, skip, take },
      },
    );
  }

  /**
   * Deactivates a session by sending a DELETE request to the API.
   *
   * @param sessionId - The unique identifier of the session to deactivate
   * @returns An Observable that completes when the session is successfully deactivated
   */
  deactivateConversation(sessionId: string): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiUrl}conversations/${sessionId}`,
    );
  }

  /**
   * Deactivates multiple sessions in bulk.
   *
   * @param request - The bulk deactivation request containing session identifiers to deactivate
   * @returns An Observable that completes when the sessions have been deactivated
   */
  deactivateConversationBulk(
    request: DeactivateSessionBulkActionDto,
  ): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}conversations/bulk`, {
      body: request,
    });
  }

  /**
   * Updates an existing session by sending a PUT request to the sessions endpoint.
   *
   * @param request - The update session request containing the session identifier and updated properties
   * @returns An Observable that emits the updated SessionDto upon successful completion
   */
  updateConversation(request: UpdateSessionActionDto): Observable<SessionDto> {
    return this.http.put<SessionDto>(
      `${environment.apiUrl}conversations`,
      request,
    );
  }

  /**
   * Loads and updates the menu sessions in the store.
   *
   * Fetches sessions based on the current menu search filter and updates the store
   * with the results. Shows an error notification if the request fails.
   * Sets the menu session searching state while the request is in progress.
   */
  loadMenuConversations(): Observable<void> {
    this.storeService.setMenuSessionSearching(true);
    return this.searchConversations(
      this.storeService.menuSessionSearchFilter(),
      0,
      this.storeService.SESSION_PAGE_SIZE,
    ).pipe(
      tap((response) => {
        this.storeService.updateMenuSessions(response.items);
      }),
      catchError(() => {
        this.notificationService.error('Error loading chats.');
        return EMPTY;
      }),
      finalize(() => this.storeService.setMenuSessionSearching(false)),
      map(() => void 0),
    );
  }

  /**
   * Loads page sessions with the specified filter and pagination parameters.
   *
   * @param filter - The search filter string to match against sessions
   * @param skip - The number of records to skip for pagination (default: 0)
   * @param take - The number of records to retrieve (default: 10)
   */
  loadPageConversations(
    filter: string,
    skip: number = 0,
    take: number = 10,
  ): Observable<void> {
    this.storeService.setPageSessionSearching(true);
    this.storeService.setPageSessionSearchFilter(filter);
    this.storeService.setPageSessionSkip(skip);
    return this.searchConversations(filter, skip, take).pipe(
      tap((response) => {
        this.handlePageConversationsResponse(
          response.items,
          response.totalCount,
        );
      }),
      catchError(() => {
        this.notificationService.error('Error loading chats.');
        return EMPTY;
      }),
      finalize(() => this.storeService.setPageSessionSearching(false)),
      map(() => void 0),
    );
  }

  /**
   * Clears all page sessions data from the store.
   *
   * Resets the page sessions array to empty, resets the skip offset to 0,
   * and sets the total count to 0.
   */
  clearPageConversations(): void {
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
   */
  handlePageConversationsResponse(
    items: SessionDto[],
    totalCount: number,
    append = false,
  ): void {
    this.storeService.setPageSessionTotalCount(totalCount);
    this.storeService.setPageSessionHasMore(
      this.storeService.pageSessionSkip() +
        this.storeService.SESSION_PAGE_SIZE <
        totalCount,
    );

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
