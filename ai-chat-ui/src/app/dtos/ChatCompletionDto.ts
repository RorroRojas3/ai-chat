export class ChatCompletionDto {
  sessionId!: string;
  message?: string;
  inputTokenCount?: number;
  outputTokenCount?: number;
  totalTokenCount?: number;
}
