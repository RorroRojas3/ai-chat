export class ChatCompletionRequestDto {
  prompt!: string;

  constructor(prompt: string) {
    this.prompt = prompt;
  }
}
