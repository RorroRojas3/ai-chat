export class SessionDto {
  id!: string;
  name!: string;
}

export class SessionMessageDto extends SessionDto {
  text!: string;
  role!: number;
}

export class SessionCoversationDto extends SessionDto {
  messages!: SessionMessageDto[];
}
