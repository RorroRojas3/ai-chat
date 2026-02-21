import { Injectable, signal } from '@angular/core';
import { MessageDto, createMessage } from '../dtos/MessageDto';
import { SessionDto } from '../dtos/SessionDto';

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  constructor() {}

  // CONSTANTS
  readonly SESSION_PAGE_SIZE = 10;

  session = signal<SessionDto | null>(null);
  isStreaming = signal<boolean>(false);
  showStreamLoader = signal<boolean>(false);
  stream = signal<string>('');
  messages = signal<MessageDto[]>([]);
  streamMessage = signal<MessageDto>(createMessage('', false, undefined));
  fileExtensions = signal<string[]>([]);
  projectId = signal<string | undefined>(undefined);

  // Search functionality
  menuSessions = signal<SessionDto[]>([]);
  menuSessionSearchFilter = signal<string>('');
  menuIsSessionSearching = signal<boolean>(false);

  pageSessions = signal<SessionDto[]>([]);
  pageSessionSearchFilter = signal<string>('');
  pageIsSessionSearching = signal<boolean>(false);
  pageSessionSkip = signal<number>(0);
  pageSessionTotalCount = signal<number>(0);
  pageSessionHasMore = signal<boolean>(true);

  /**
   * Resets all store states to their initial values, preparing for a new chat session.
   * This includes clearing the session ID, message history, prompt button state,
   * streaming flags and current stream content.
   *
   * @remarks
   * This method performs a complete reset of the store by:
   * - Clearing the session identifier
   * - Emptying the messages array
   * - Enabling the prompt button
   * - Resetting streaming states
   * - Clearing stream content
   * - Initializing a new empty stream message
   */
  clearForNewSession(): void {
    this.session.set(null);
    this.messages.set([]);
    this.isStreaming.set(false);
    this.showStreamLoader.set(false);
    this.stream.set('');
    this.streamMessage.set(createMessage('', false, undefined));
  }

  /**
   * Sets the search filter for menu sessions
   *
   * @param query - The search query string to filter sessions by
   */
  setMenuSessionSearchFilter(query: string): void {
    this.menuSessionSearchFilter.set(query);
  }

  /**
   * Clears the menu session search filter, resetting it to an empty string
   */
  clearMenuSessionSearchFilter(): void {
    this.menuSessionSearchFilter.set('');
  }

  /**
   * Sets the searching state for menu sessions
   *
   * @param isSearching - Boolean flag indicating whether a search operation is in progress
   */
  setMenuSessionSearching(isSearching: boolean): void {
    this.menuIsSessionSearching.set(isSearching);
  }

  /**
   * Updates the menu sessions with the provided array of session data transfer objects
   *
   * @param sessions - An array of SessionDto objects to set as the current menu sessions
   */
  updateMenuSessions(sessions: SessionDto[]): void {
    this.menuSessions.set(sessions);
  }

  /**
   * Sets the search filter for page sessions
   *
   * @param query - The search query string to filter sessions by
   */
  setPageSessionSearchFilter(query: string): void {
    this.pageSessionSearchFilter.set(query);
  }

  /**
   * Clears the page session search filter, resetting it to an empty string
   */
  clearPageSessionSearchFilter(): void {
    this.pageSessionSearchFilter.set('');
  }

  /**
   * Sets the searching state for page sessions
   *
   * @param isSearching - Boolean flag indicating whether a search operation is in progress
   */
  setPageSessionSearching(isSearching: boolean): void {
    this.pageIsSessionSearching.set(isSearching);
  }

  /**
   * Sets the skip offset for page session pagination
   *
   * @param skip - The number of sessions to skip when fetching the next page
   */
  setPageSessionSkip(skip: number): void {
    this.pageSessionSkip.set(skip);
  }

  /**
   * Sets the total count of page sessions available
   *
   * @param totalCount - The total number of sessions available for pagination
   */
  setPageSessionTotalCount(totalCount: number): void {
    this.pageSessionTotalCount.set(totalCount);
  }

  /**
   * Sets whether there are more page sessions available to load
   *
   * @param hasMore - Boolean flag indicating if additional sessions can be loaded
   */
  setPageSessionHasMore(hasMore: boolean): void {
    this.pageSessionHasMore.set(hasMore);
  }

  /**
   * Updates the page sessions with the provided array of session data transfer objects
   *
   * @param sessions - An array of SessionDto objects to set as the current page sessions
   */
  updatePageSessions(sessions: SessionDto[]): void {
    this.pageSessions.set(sessions);
  }
}
