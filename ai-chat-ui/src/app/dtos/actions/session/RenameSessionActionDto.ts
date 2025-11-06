export class RenameSessionActionDto {
  id!: string;
  name!: string;

  constructor(sessionId?: string, sessionName?: string) {
    this.id = sessionId!;
    this.name = sessionName!;
  }
}
