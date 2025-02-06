export class ChatStreamRequestDto {
  prompt!: string;

  constructor(prompt: string) {
    this.prompt = prompt;
  }
}
