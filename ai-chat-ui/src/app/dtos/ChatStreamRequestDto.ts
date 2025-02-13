export class ChatStreamRequestDto {
  prompt!: string;
  modelId!: string;

  constructor(prompt: string, modelId: string) {
    this.prompt = prompt;
    this.modelId = modelId;
  }
}
