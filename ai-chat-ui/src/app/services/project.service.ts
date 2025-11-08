import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ProjectDto } from '../dtos/ProjectDto';
import { PaginatedResponseDto } from '../dtos/PaginatedResponseDto';
import { UpsertProjectActionDto } from '../dtos/actions/project/UpsertProjectActionDto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly http = inject(HttpClient);

  createProject(request: UpsertProjectActionDto): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(`${environment.apiUrl}projects`, request);
  }

  updateProject(request: UpsertProjectActionDto): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`${environment.apiUrl}projects`, request);
  }

  searchProjects(
    filter: string,
    skip: number = 0,
    take: number = 10
  ): Observable<PaginatedResponseDto<ProjectDto>> {
    return this.http.get<PaginatedResponseDto<ProjectDto>>(
      `${environment.apiUrl}projects/search`,
      {
        params: { filter, skip, take },
      }
    );
  }

  deactivateProject(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}projects/${id}`);
  }
}
