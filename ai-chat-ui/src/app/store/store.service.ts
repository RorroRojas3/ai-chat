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

  clearForNewSession(): void {
    this.sessionId.set('');
    this.messages.set([]);
    this.disablePromptButton.set(false);
    this.isStreaming.set(false);
    this.stream.set('');
    this.streamMessage.set(new MessageDto('', false, undefined));
  }
}
