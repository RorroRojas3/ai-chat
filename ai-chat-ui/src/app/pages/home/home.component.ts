import { Component, OnInit, signal } from '@angular/core';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';
import { MarkdownViewerComponent } from '../../components/markdown-viewer/markdown-viewer.component';
import { StoreService } from '../../store/store.service';
import { MessageBubbleComponent } from '../../components/home/message-bubble/message-bubble.component';
import { NgIf } from '@angular/common';
import { MessageDto } from '../../dtos/MessageDto';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-home',
  imports: [
    PromptBoxComponent,
    MarkdownViewerComponent,
    MessageBubbleComponent,
    NgIf,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  markdown?: SafeHtml;
  stream?: string;
  messages = signal<MessageDto[]>([]);
  md: markdownit;

  constructor(
    public storeService: StoreService,
    private sanitizer: DomSanitizer
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    });
  }

  ngOnInit(): void {
    const html = this.md.render('Nelly, would you be my valentine?');
    const sanitzeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
    this.storeService.messages.update((messages) => [
      ...messages,
      new MessageDto('', false, sanitzeHtml),
    ]);
  }

  onMarkdownChange(markdown: SafeHtml): void {
    this.markdown = markdown;
    this.storeService.streamMessage.update((streamMessage) => ({
      ...streamMessage,
      markdown,
    }));
  }

  onStreamChange(stream: string): void {
    this.stream = stream;
  }
}
