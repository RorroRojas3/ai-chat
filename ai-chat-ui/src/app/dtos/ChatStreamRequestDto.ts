export class ChatStreamRequestDto {
  prompt!: string;
  modelId!: string;
  serviceId!: string;

  constructor(prompt: string, modelId: string, serviceId: string) {
    this.prompt = prompt;
    this.modelId = modelId;
    this.serviceId = serviceId;
  }
}
