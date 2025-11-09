export class CreateSessionActioDto {
  projectId?: string;

  constructor(projectId?: string) {
    this.projectId = projectId;
  }
}
