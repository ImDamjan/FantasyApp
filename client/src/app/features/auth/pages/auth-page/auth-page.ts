import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { passwordMatchValidator } from '../../../../core/validators/password-match.validator';
import { FormField } from '../../../../shared/components/form-field/form-field';

type AuthMode = 'login' | 'register';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormField],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
})
export class AuthPage {
  private readonly fb = inject(FormBuilder);

  readonly mode = signal<AuthMode>('login');
  readonly loading = signal(false);
  readonly loginError = signal<string | null>(null);
  readonly registerError = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  readonly registerForm = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator('password', 'confirmPassword') },
  );

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  setMode(mode: AuthMode): void {
    this.mode.set(mode);
    this.loginError.set(null);
    this.registerError.set(null);
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.loginError.set(null);

    this.authService
      .login(this.loginForm.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => this.loginError.set(err?.error?.message ?? 'Prijava nije uspela.'),
      });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.registerError.set(null);

    this.authService
      .register(this.registerForm.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => this.registerError.set(err?.error?.message ?? 'Registracija nije uspela.'),
      });
  }
}
