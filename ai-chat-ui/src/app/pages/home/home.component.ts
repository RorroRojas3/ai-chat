import { Component } from '@angular/core';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';
import { MarkdownViewerComponent } from '../../components/markdown-viewer/markdown-viewer.component';
import { SafeHtml } from '@angular/platform-browser';
import { StoreService } from '../../store/store.service';

@Component({
  selector: 'app-home',
  imports: [PromptBoxComponent, MarkdownViewerComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  markdown?: SafeHtml;
  stream?: string;

  constructor(public storeService: StoreService) {}

  onMarkdownChange(markdown: SafeHtml): void {
    this.markdown = markdown;
  }

  onStreamChange(stream: string): void {
    this.stream = stream;
  }
}
