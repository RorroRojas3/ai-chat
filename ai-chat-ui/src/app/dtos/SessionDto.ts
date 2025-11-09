export interface SessionDto {
  id: string;
  name: string;
  projectId?: string;
  dateCreated: string; // ISO date string from API
  dateModified: string; // ISO date string from API
}

export interface SessionMessageDto {
  id: string;
  name: string;
  dateCreated: string;
  dateModified: string;
  text: string;
  role: number;
}

export interface SessionConversationDto {
  id: string;
  name: string;
  dateCreated: string;
  dateModified: string;
  messages: SessionMessageDto[];
}
