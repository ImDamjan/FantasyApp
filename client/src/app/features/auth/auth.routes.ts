import { Routes } from '@angular/router';
import { AuthPage } from './pages/auth-page/auth-page';
import { ForgotPasswordPage } from './pages/forgot-password-page/forgot-password-page';
import { ResetPasswordPage } from './pages/reset-password-page/reset-password-page';

export const AUTH_ROUTES: Routes = [
  { path: '', component: AuthPage },
  { path: 'forgot-password', component: ForgotPasswordPage },
  { path: 'reset-password', component: ResetPasswordPage },
];
