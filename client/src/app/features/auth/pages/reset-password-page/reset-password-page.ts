import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../core/services/toast.service';
import { passwordMatchValidator } from '../../../../core/validators/password-match.validator';
import { FormField } from '../../../../shared/components/form-field/form-field';
import { Logo } from '../../../../shared/components/logo/logo';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormField, Logo],
  templateUrl: './reset-password-page.html',
  styleUrl: './reset-password-page.scss',
})
export class ResetPasswordPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  private readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly linkInvalid = !this.email || !this.token;
  readonly loading = signal(false);
  readonly submitted = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required]],
      confirmNewPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator('newPassword', 'confirmNewPassword') },
  );

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.authService
      .resetPassword({ email: this.email, token: this.token, ...this.form.getRawValue() })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.submitted.set(true),
        error: (err) => this.toastService.error(err?.error?.message ?? 'Password reset failed.'),
      });
  }
}
