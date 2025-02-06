import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  constructor() {}

  sessionId = signal<string>('');
  disablePromptButton = signal<boolean>(false);
  stream = signal<string>('');
}
