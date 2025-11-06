import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
} from '@angular/core';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-session-rename-modal',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './session-rename-modal.component.html',
  styleUrl: './session-rename-modal.component.scss',
})
export class SessionRenameModalComponent implements OnChanges {
  @Input() show: boolean = false;
  @Input() name: string = '';
  @Output() onRename = new EventEmitter<string>();
  @Output() onClose = new EventEmitter<void>();

  sessionName = new FormControl('', [
    Validators.required,
    Validators.minLength(1),
    Validators.maxLength(256),
  ]);

  /**
   * Lifecycle hook that responds to changes in input properties.
   * Resets the session name form control when the modal is shown.
   */
  ngOnChanges(): void {
    if (this.show) {
      this.sessionName.setValue(this.name);
      this.sessionName.markAsUntouched();
      this.sessionName.markAsPristine();
    }
  }

  /**
   * Determines whether to display validation errors.
   * Returns true if the form control is invalid and has been interacted with.
   */
  showError(): boolean {
    return (
      this.sessionName.invalid &&
      (this.sessionName.dirty || this.sessionName.touched)
    );
  }

  /**
   * Gets the current character count of the trimmed session name.
   * Returns 0 if the value is empty or null.
   */
  characterCount(): number {
    return this.sessionName.value?.trim().length || 0;
  }

  /**
   * Handles the rename action by emitting the trimmed session name value.
   */
  handleRename(): void {
    this.onRename.emit(this.sessionName.value!.trim());
  }

  /**
   * Handles the modal close action by emitting the close event.
   */
  handleClose(): void {
    this.onClose.emit();
  }
}
