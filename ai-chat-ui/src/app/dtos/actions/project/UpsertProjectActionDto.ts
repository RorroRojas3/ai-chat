export interface UpsertProjectActionDto {
  id?: string;
  sessionId: string;
  name: string;
  description: string;
  instructions: string;
}
