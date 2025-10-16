export class SessionDto {
  id!: string;
  name!: string;
  dateCreated!: Date;
  dateModified!: Date;

  constructor(init?: Partial<SessionDto>) {
    if (init) {
      Object.assign(this, init);
    }
  }
}

export class SessionMessageDto extends SessionDto {
  text!: string;
  role!: number;
}

export class SessionCoversationDto extends SessionDto {
  messages!: SessionMessageDto[];
}
