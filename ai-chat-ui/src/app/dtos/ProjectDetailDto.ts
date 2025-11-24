import { SessionDto } from './SessionDto';
import { ProjectDto } from './ProjectDto';

export interface ProjectDetailDto extends ProjectDto {
  sessions: SessionDto[];
}
