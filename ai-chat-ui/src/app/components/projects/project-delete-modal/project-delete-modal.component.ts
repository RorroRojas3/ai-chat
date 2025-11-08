import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-project-delete-modal',
  imports: [],
  templateUrl: './project-delete-modal.component.html',
  styleUrl: './project-delete-modal.component.scss',
})
export class ProjectDeleteModalComponent {
  show = input.required<boolean>();
  projectName = input<string>('');

  onDelete = output<void>();
  onClose = output<void>();

  confirmDelete(): void {
    this.onDelete.emit();
  }

  closeModal(): void {
    this.onClose.emit();
  }
}
