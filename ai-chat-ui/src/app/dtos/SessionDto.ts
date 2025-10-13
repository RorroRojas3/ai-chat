export class SessionDto {
  id!: string;
  name!: string;
  dateCreated!: Date;
  dateModified!: Date;

  /**
   * Gets the dateCreated converted from UTC to local time
   */
  getLocalDateCreated(): Date {
    return new Date(this.dateCreated);
  }

  /**
   * Gets the dateModified converted from UTC to local time
   */
  getLocalDateModified(): Date {
    return new Date(this.dateModified);
  }
}

export class SessionMessageDto extends SessionDto {
  text!: string;
  role!: number;
}

export class SessionCoversationDto extends SessionDto {
  messages!: SessionMessageDto[];
}
