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

  ngOnChanges(): void {
    if (this.show) {
      this.sessionName.setValue(this.name);
      this.sessionName.markAsUntouched();
      this.sessionName.markAsPristine();
    }
  }

  get showError(): boolean {
    return (
      this.sessionName.invalid &&
      (this.sessionName.dirty || this.sessionName.touched)
    );
  }

  get characterCount(): number {
    return this.sessionName.value?.trim().length || 0;
  }

  handleRename(): void {
    this.onRename.emit(this.sessionName.value!.trim());
  }

  handleClose(): void {
    this.onClose.emit();
  }
}
