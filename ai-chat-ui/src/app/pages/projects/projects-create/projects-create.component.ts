import { Component, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, tap, EMPTY } from 'rxjs';
import { ProjectService } from '../../../services/project.service';
import { UpsertProjectActionDto } from '../../../dtos/actions/project/UpsertProjectActionDto';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-projects-create',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './projects-create.component.html',
  styleUrl: './projects-create.component.scss',
})
export class ProjectsCreateComponent {
  // Constants
  readonly MAX_NAME_LENGTH = 256;
  readonly MAX_DESCRIPTION_LENGTH = 1024;

  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  isSubmitting = false;

  projectForm = new FormGroup({
    name: new FormControl('', [
      Validators.required,
      Validators.maxLength(this.MAX_NAME_LENGTH),
    ]),
    description: new FormControl('', [
      Validators.required,
      Validators.maxLength(this.MAX_DESCRIPTION_LENGTH),
    ]),
  });

  /**
   * Gets the name form control for easier access in template
   */
  get nameControl(): FormControl {
    return this.projectForm.get('name') as FormControl;
  }

  /**
   * Gets the description form control for easier access in template
   */
  get descriptionControl(): FormControl {
    return this.projectForm.get('description') as FormControl;
  }

  /**
   * Determines whether to display validation errors for name field.
   */
  showNameError(): boolean {
    return (
      this.nameControl.invalid &&
      (this.nameControl.dirty || this.nameControl.touched)
    );
  }

  /**
   * Determines whether to display validation errors for description field.
   */
  showDescriptionError(): boolean {
    return (
      this.descriptionControl.invalid &&
      (this.descriptionControl.dirty || this.descriptionControl.touched)
    );
  }

  /**
   * Gets the current character count of the trimmed name.
   */
  nameCharacterCount(): number {
    return this.nameControl.value?.trim().length || 0;
  }

  /**
   * Gets the current character count of the trimmed description.
   */
  descriptionCharacterCount(): number {
    return this.descriptionControl.value?.trim().length || 0;
  }

  /**
   * Handles the form submission by creating a new project
   */
  handleSubmit(): void {
    if (this.projectForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;

      const request: UpsertProjectActionDto = {
        name: this.nameControl.value!.trim(),
        description: this.descriptionControl.value!.trim(),
        instructions: '', // Default empty instructions as per DTO
      };

      this.projectService
        .createProject(request)
        .pipe(
          tap((project) => {
            this.notificationService.success(
              `Project "${project.name}" created successfully!`
            );
            this.router.navigate(['/projects', project.id]);
          }),
          catchError(() => {
            this.notificationService.error(
              'Failed to create project. Please try again.'
            );
            this.isSubmitting = false;
            return EMPTY;
          }),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe();
    }
  }

  /**
   * Handles the cancel action by navigating back to projects list
   */
  handleCancel(): void {
    this.router.navigate(['/projects']);
  }
}
