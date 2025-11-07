import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-session-delete-modal',
  imports: [CommonModule],
  templateUrl: './session-delete-modal.component.html',
  styleUrl: './session-delete-modal.component.scss',
})
export class SessionDeleteModalComponent {
  show = input<boolean>(false);
  sessionCount = input<number>(0);
  onDelete = output<void>();
  onClose = output<void>();

  /**
   * Handles the delete action by emitting the delete event.
   */
  handleDelete(): void {
    this.onDelete.emit();
  }

  /**
   * Handles the modal close action by emitting the close event.
   */
  handleClose(): void {
    this.onClose.emit();
  }
}
