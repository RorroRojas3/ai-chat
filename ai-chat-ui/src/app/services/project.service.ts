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

  /**
   * Creates a new project by sending a POST request to the API.
   *
   * @param request - The project data to be created, conforming to UpsertProjectActionDto.
   * @returns An Observable that emits the created ProjectDto object.
   *
   * @remarks
   * This method sends an HTTP POST request to the `/projects` endpoint.
   * The response will contain the newly created project details.
   */
  createProject(request: UpsertProjectActionDto): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(`${environment.apiUrl}projects`, request);
  }

  /**
   * Updates an existing project with the provided data.
   *
   * @param request - The project data to update, containing all necessary fields for the update operation
   * @returns An Observable that emits the updated ProjectDto upon successful completion
   *
   * @remarks
   * This method sends a PUT request to the projects endpoint of the API.
   * The request is asynchronous and returns an Observable that must be subscribed to in order to execute.
   */
  updateProject(request: UpsertProjectActionDto): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`${environment.apiUrl}projects`, request);
  }

  /**
   * Searches for projects based on the provided filter criteria.
   *
   * @param filter - The search filter string to match against projects
   * @param skip - The number of records to skip for pagination (default: 0)
   * @param take - The number of records to retrieve (default: 10)
   * @returns An Observable that emits a paginated response containing matching ProjectDto objects
   */
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

  /**
   * Deactivates a project by its ID.
   *
   * Sends a DELETE request to the projects API endpoint to deactivate the specified project.
   *
   * @param id - The unique identifier of the project to deactivate
   * @returns An Observable that completes when the project has been successfully deactivated
   *
   * @remarks
   * This method performs a soft delete by deactivating the project rather than permanently removing it.
   * The operation is asynchronous and returns an Observable that emits void upon completion.
   */
  deactivateProject(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}projects/${id}`);
  }
}
