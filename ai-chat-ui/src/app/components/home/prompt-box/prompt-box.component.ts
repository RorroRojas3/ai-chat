import { Component } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-prompt-box',
  imports: [FormsModule],
  templateUrl: './prompt-box.component.html',
  styleUrl: './prompt-box.component.scss',
})
export class PromptBoxComponent {
  constructor(
    public storeService: StoreService,
    private chatService: ChatService
  ) {}

  prompt: string = '';

  onClickCreateSession(): void {
    if (this.prompt !== '') {
      this.storeService.disablePromptButton.set(true);
      this.chatService.createSession().subscribe((session) => {
        this.storeService.sessionId.set(session.sessionId);
        this.prompt = '';
      });
    }
  }
}
