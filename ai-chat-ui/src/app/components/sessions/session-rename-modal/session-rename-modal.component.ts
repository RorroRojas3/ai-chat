import { Component, input, output, effect, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';


@Component({
  selector: 'app-session-rename-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './session-rename-modal.component.html',
  styleUrl: './session-rename-modal.component.scss',
})
export class SessionRenameModalComponent {
  // Constants
  readonly MAX_NAME_LENGTH = 256;

  show = input<boolean>(false);
  name = input<string>('');
  onRename = output<string>();
  onClose = output<void>();
  shouldShow = signal<boolean>(false);

  sessionName = new FormControl('', [
    Validators.required,
    Validators.minLength(1),
    Validators.maxLength(this.MAX_NAME_LENGTH),
  ]);

  constructor() {
    // Use effect to respond to input changes
    effect(() => {
      if (this.show()) {
        this.sessionName.setValue(this.name());
        this.sessionName.markAsUntouched();
        this.sessionName.markAsPristine();
        setTimeout(() => {
          this.shouldShow.set(true);
        }, 10);
      } else {
        this.shouldShow.set(false);
      }
    });
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
    if (this.sessionName.valid && this.sessionName.value) {
      this.onRename.emit(this.sessionName.value.trim());
    }
  }

  /**
   * Handles the modal close action by emitting the close event.
   */
  handleClose(): void {
    this.onClose.emit();
  }
}
