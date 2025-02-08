import { SafeHtml } from '@angular/platform-browser';

export class MessageDto {
  content?: string;
  isOutgoing!: boolean;
  markdown?: SafeHtml;

  constructor(content: string, isOutgoing: boolean, markdown?: SafeHtml) {
    this.content = content;
    this.isOutgoing = isOutgoing;
    this.markdown = markdown;
  }
}
