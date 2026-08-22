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

    if (errors['required']) return `Polje "${this.label}" je obavezno.`;
    if (errors['email']) return 'Unesi ispravnu email adresu.';
    if (errors['minlength']) {
      return `Minimum ${errors['minlength'].requiredLength} karaktera.`;
    }
    if (errors['mismatch']) return 'Lozinke se ne poklapaju.';

    return 'Neispravna vrednost.';
  }
}
