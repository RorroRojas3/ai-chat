import { Component, EventEmitter, Output } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-prompt-box',
  imports: [FormsModule],
  templateUrl: './prompt-box.component.html',
  styleUrl: './prompt-box.component.scss',
})
export class PromptBoxComponent {
  constructor(
    public storeService: StoreService,
    private chatService: ChatService,
    private sanitizer: DomSanitizer
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    });
  }

  prompt: string = '';
  message?: string = '';
  md: markdownit;
  @Output() markdown = new EventEmitter<SafeHtml>();

  async onClickCreateSession(): Promise<void> {
    if (!this.prompt.trim()) {
      return;
    }

    try {
      this.storeService.disablePromptButton.set(true);

      if (this.storeService.sessionId() === '') {
        const session = await firstValueFrom(this.chatService.createSession());
        this.storeService.sessionId.set(session.id);
      }

      const response = await firstValueFrom(
        this.chatService.createCompleteMessage(this.prompt)
      );
      this.message = response.message;
      const html = this.md.render(this.message ?? '');
      const sanitzeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
      this.markdown.emit(sanitzeHtml);
      this.prompt = '';
    } catch (error) {
      // Handle errors appropriately
      console.error('Error in session creation:', error);
      // You might want to show an error message to the user
    } finally {
      // Re-enable the button regardless of success/failure
      this.storeService.disablePromptButton.set(false);
    }
  }
}
