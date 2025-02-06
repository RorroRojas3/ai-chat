import { Component } from '@angular/core';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';
import { MarkdownViewerComponent } from '../../components/markdown-viewer/markdown-viewer.component';
import { SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-home',
  imports: [PromptBoxComponent, MarkdownViewerComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  markdown?: SafeHtml;

  onMarkdownChange(markdown: SafeHtml): void {
    this.markdown = markdown;
  }
}
