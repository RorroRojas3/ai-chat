import { Component, Input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import markdownit from 'markdown-it';

@Component({
  selector: 'app-markdown-viewer',
  imports: [],
  templateUrl: './markdown-viewer.component.html',
  styleUrl: './markdown-viewer.component.scss',
})
export class MarkdownViewerComponent {
  @Input() markdown?: SafeHtml;
}
