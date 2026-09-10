import { Component, Input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './form-field.html',
  styleUrl: './form-field.scss',
})
export class FormField {
  @Input({ required: true }) control!: FormControl;
  @Input({ required: true }) label!: string;
  @Input() type: string = 'text';
  @Input() placeholder: string = '';
  @Input() autocomplete: string = 'off';

  get errorMessage(): string | null {
    if (!this.control.touched || !this.control.errors) {
      return null;
    }

    const errors = this.control.errors;

    if (errors['required']) return `${this.label} is required.`;
    if (errors['email']) return 'Enter a valid email address.';
    if (errors['minlength']) {
      return `Minimum ${errors['minlength'].requiredLength} characters.`;
    }
    if (errors['mismatch']) return 'Passwords do not match.';

    return 'Invalid value.';
  }
}
