import { Injectable, signal } from '@angular/core';
import { MessageDto } from '../dtos/MessageDto';
import { ModelDto } from '../dtos/ModelDto';
import { SessionDto } from '../dtos/SessionDto';

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  constructor() {}

  sessionId = signal<string>('');
  disablePromptButton = signal<boolean>(false);
  isStreaming = signal<boolean>(false);
  stream = signal<string>('');
  messages = signal<MessageDto[]>([]);
  streamMessage = signal<MessageDto>(new MessageDto('', false, undefined));
  models = signal<ModelDto[]>([]);
  selectedModelId = signal<string>('');
  sessions = signal<SessionDto[]>([]);

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
    this.sessionId.set('');
    this.messages.set([]);
    this.disablePromptButton.set(false);
    this.isStreaming.set(false);
    this.stream.set('');
    this.streamMessage.set(new MessageDto('', false, undefined));
  }
}
