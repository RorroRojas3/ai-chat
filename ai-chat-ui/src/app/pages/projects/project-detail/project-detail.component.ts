import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../../../services/project.service';
import { ProjectDetailDto } from '../../../dtos/ProjectDetailDto';
import { NotificationService } from '../../../services/notification.service';
import { catchError, of } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-project-detail',
  imports: [],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent implements OnInit {
  projectService = inject(ProjectService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  notificationService = inject(NotificationService);
  project = signal<ProjectDetailDto | undefined>(undefined);

  projectId: string | null = null;

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id');

    if (!this.projectId) {
      this.notificationService.error('Project ID is required');
      return;
    }

    this.projectService
      .getProjectById(this.projectId)
      .pipe(
        catchError(() => {
          this.notificationService.error(
            `Error getting project details for id ${this.projectId}`
          );
          return of(undefined);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((project) => {
        if (project) {
          this.project.set(project);
        }
      });
  }
}
