import { Injectable, signal } from '@angular/core';
import { MessageDto } from '../dtos/MessageDto';

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
}
