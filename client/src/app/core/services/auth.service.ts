import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, of, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import {
  AuthResponse,
  AuthUser,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
} from '../models/auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSignal = signal<AuthUser | null>(null);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  constructor(
    private readonly http: HttpClient,
    private readonly tokenStorage: TokenStorageService,
  ) {
    const storedUser = this.tokenStorage.getUser();
    if (storedUser && this.tokenStorage.hasSession()) {
      this.currentUserSignal.set(storedUser);
    }
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(API_ENDPOINTS.auth.register, request)
      .pipe(tap((response) => this.persistSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(API_ENDPOINTS.auth.login, request)
      .pipe(tap((response) => this.persistSession(response)));
  }

  refreshAccessToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    return this.http
      .post<AuthResponse>(API_ENDPOINTS.auth.refreshToken, { refreshToken })
      .pipe(tap((response) => this.persistSession(response)));
  }

  logout(): Observable<unknown> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    return this.http.post(API_ENDPOINTS.auth.revokeToken, { refreshToken }).pipe(
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<unknown> {
    return this.http.post(API_ENDPOINTS.auth.forgotPassword, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<unknown> {
    return this.http.post(API_ENDPOINTS.auth.resetPassword, request);
  }

  clearSession(): void {
    this.tokenStorage.clear();
    this.currentUserSignal.set(null);
  }

  private persistSession(response: AuthResponse): void {
    this.tokenStorage.setAccessToken(response.accessToken);
    this.tokenStorage.setRefreshToken(response.refreshToken);
    const user: AuthUser = {
      userId: response.userId,
      email: response.email,
      username: response.username,
    };
    this.tokenStorage.setUser(user);
    this.currentUserSignal.set(user);
  }
}
