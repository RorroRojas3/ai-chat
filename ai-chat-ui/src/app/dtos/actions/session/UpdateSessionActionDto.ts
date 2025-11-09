export class UpdateSessionActionDto {
  id!: string;
  name!: string;
  projectId?: string;

  constructor(sessionId?: string, sessionName?: string, projectId?: string) {
    this.id = sessionId!;
    this.name = sessionName!;
    this.projectId = projectId;
  }
}
