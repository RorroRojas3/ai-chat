import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { ProjectDto } from '../../dtos/ProjectDto';
import { PaginatedResponseDto } from '../../dtos/PaginatedResponseDto';
import {
  debounceTime,
  distinctUntilChanged,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProjectDeleteModalComponent } from '../../components/projects/project-delete-modal/project-delete-modal.component';

@Component({
  selector: 'app-projects',
  imports: [CommonModule, FormsModule, ProjectDeleteModalComponent],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
})
export class ProjectsComponent implements OnInit {
  // Constants
  readonly PAGE_SIZE = 10;
  readonly SEARCH_DEBOUNCE_MS = 600;

  // Inject dependencies using Angular 19 pattern
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  projects: ProjectDto[] = [];
  searchFilter = '';
  isSearching = false;
  isLoadingMore = false;

  // Virtual scrolling properties
  currentSkip = 0;
  totalCount = 0;
  hasMoreProjects = true;
  showDeleteModal = false;
  selectedProjectId: string | null = null;

  // Hover tracking
  hoveredProjectId: string | null = null;

  private searchSubject = new Subject<string>();

  constructor() {
    // Set up debounced search with switchMap to avoid race conditions
    this.searchSubject
      .pipe(
        debounceTime(this.SEARCH_DEBOUNCE_MS),
        distinctUntilChanged(),
        tap(() => {
          this.isSearching = true;
          this.currentSkip = 0; // Reset to beginning on new search
          this.projects = []; // Clear existing projects on new search
        }),
        switchMap((filter) => {
          this.searchFilter = filter;
          return this.projectService.searchProjects(
            filter,
            this.currentSkip,
            this.PAGE_SIZE
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          this.handleProjectsResponse(response);
          this.isSearching = false;
        },
        error: () => {
          this.isSearching = false;
        },
      });
  }

  ngOnInit(): void {
    this.loadProjects();
  }

  /**
   * Handles project response and updates component state
   */
  private handleProjectsResponse(
    response: PaginatedResponseDto<ProjectDto>,
    append = false
  ): void {
    this.totalCount = response.totalCount;
    this.hasMoreProjects = response.items.length === this.PAGE_SIZE;

    if (append) {
      this.projects = [...this.projects, ...response.items];
    } else {
      this.projects = response.items;
    }
  }

  /**
   * Loads initial projects
   */
  private loadProjects(): void {
    this.isSearching = true;
    this.currentSkip = 0;
    this.projects = [];

    this.projectService
      .searchProjects(this.searchFilter, this.currentSkip, this.PAGE_SIZE)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.handleProjectsResponse(response);
          this.isSearching = false;
        },
        error: () => {
          this.isSearching = false;
        },
      });
  }

  /**
   * Handles search input changes
   */
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  /**
   * Clears the search term and reloads projects
   */
  clearSearch(): void {
    this.searchFilter = '';
    this.loadProjects();
  }

  /**
   * TrackBy function for project list to improve performance
   */
  trackByProjectId(_index: number, project: ProjectDto): string {
    return project.id;
  }

  /**
   * Loads more projects (next page) and appends them to the existing list
   */
  loadMoreProjects(): void {
    if (!this.hasMoreProjects || this.isLoadingMore) {
      return;
    }

    this.isLoadingMore = true;
    this.currentSkip += this.PAGE_SIZE;

    this.projectService
      .searchProjects(this.searchFilter, this.currentSkip, this.PAGE_SIZE)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.handleProjectsResponse(response, true);
          this.isLoadingMore = false;
        },
        error: () => {
          this.isLoadingMore = false;
        },
      });
  }

  /**
   * Navigates to the selected project details page
   */
  onClickProject(projectId: string): void {
    this.router.navigate(['projects', projectId]);
  }

  /**
   * Navigates to create new project page
   */
  onClickCreateNewProject(): void {
    this.router.navigate(['projects', 'create']);
  }

  /**
   * Navigates to edit project page
   */
  onEditProject(projectId: string, event?: Event): void {
    event?.stopPropagation();
    this.router.navigate(['projects', 'edit', projectId]);
  }

  /**
   * Initiates deletion of a single project
   */
  onDeleteProject(projectId: string, event?: Event): void {
    event?.stopPropagation();
    this.selectedProjectId = projectId;
    this.showDeleteModal = true;
  }

  /**
   * Handles the deletion of the selected project
   */
  handleDeleteProject(): void {
    if (!this.selectedProjectId) {
      this.closeDeleteModal();
      return;
    }

    this.projectService
      .deactivateProject(this.selectedProjectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.handleDeleteSuccess();
        },
        error: () => {
          this.closeDeleteModal();
        },
      });
  }

  /**
   * Handles successful deletion by reloading projects
   */
  private handleDeleteSuccess(): void {
    this.closeDeleteModal();
    this.loadProjects();
  }

  /**
   * Closes the delete modal and resets state
   */
  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedProjectId = null;
  }

  /**
   * Handles mouse enter on a project item
   */
  onProjectMouseEnter(projectId: string): void {
    this.hoveredProjectId = projectId;
  }

  /**
   * Handles mouse leave on a project item
   */
  onProjectMouseLeave(): void {
    this.hoveredProjectId = null;
  }

  /**
   * Gets the name of the selected project for display in delete modal
   */
  getSelectedProjectName(): string {
    if (!this.selectedProjectId) {
      return '';
    }
    const project = this.projects.find((p) => p.id === this.selectedProjectId);
    return project?.name || '';
  }
}
