import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-session-delete-modal',
  imports: [CommonModule],
  templateUrl: './session-delete-modal.component.html',
  styleUrl: './session-delete-modal.component.scss',
})
export class SessionDeleteModalComponent {
  @Input() show: boolean = false;
  @Input() sessionCount: number = 0;
  @Output() onDelete = new EventEmitter<void>();
  @Output() onClose = new EventEmitter<void>();

  handleDelete(): void {
    this.onDelete.emit();
  }

  handleClose(): void {
    this.onClose.emit();
  }
}
