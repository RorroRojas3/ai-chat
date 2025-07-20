import { Component, computed, signal } from '@angular/core';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';
import { StoreService } from '../../store/store.service';
import { MessageBubbleComponent } from '../../components/home/message-bubble/message-bubble.component';
import { NgIf } from '@angular/common';
import { MessageDto } from '../../dtos/MessageDto';
import markdownit from 'markdown-it';
import { SafeHtml } from '@angular/platform-browser';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import hljs from 'highlight.js';

@Component({
  selector: 'app-home',
  imports: [PromptBoxComponent, MessageBubbleComponent, NgIf],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  markdown?: SafeHtml;
  stream?: string;
  messages = signal<MessageDto[]>([]);
  md: markdownit;

  canHighlight = computed(() => {
    return this.storeService.isStreaming();
  });

  constructor(public storeService: StoreService) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });
  }

  /**
   * Handles changes to the markdown content by updating the local markdown property
   * and synchronizing the change with the stream message in the store service.
   *
   * @param markdown - The updated markdown content as SafeHtml
   */
  onMarkdownChange(markdown: SafeHtml): void {
    this.markdown = markdown;
    this.storeService.streamMessage.update((streamMessage) => ({
      ...streamMessage,
      markdown,
    }));
  }
}
