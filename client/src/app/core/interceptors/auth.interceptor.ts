import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject, catchError, switchMap, take, throwError } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

const AUTH_ENDPOINTS: string[] = [
  API_ENDPOINTS.auth.login,
  API_ENDPOINTS.auth.register,
  API_ENDPOINTS.auth.refreshToken,
  API_ENDPOINTS.auth.forgotPassword,
  API_ENDPOINTS.auth.resetPassword,
];

let isRefreshing = false;
const refreshedToken$ = new Subject<string>();

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  const authService = inject(AuthService);
  const router = inject(Router);

  const isAuthEndpoint = AUTH_ENDPOINTS.some((endpoint) => req.url === endpoint);
  const accessToken = tokenStorage.getAccessToken();

  const authorizedReq =
    accessToken && !isAuthEndpoint
      ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
      : req;

  return next(authorizedReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isAuthEndpoint
      ) {
        return handleUnauthorized(authorizedReq, next, authService, router);
      }
      return throwError(() => error);
    }),
  );
};

function handleUnauthorized(
  req: Parameters<HttpInterceptorFn>[0],
  next: Parameters<HttpInterceptorFn>[1],
  authService: AuthService,
  router: Router,
): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;

    return authService.refreshAccessToken().pipe(
      switchMap((response) => {
        isRefreshing = false;
        refreshedToken$.next(response.accessToken);
        const retriedReq = req.clone({
          setHeaders: { Authorization: `Bearer ${response.accessToken}` },
        });
        return next(retriedReq);
      }),
      catchError((refreshError: unknown) => {
        isRefreshing = false;
        authService.clearSession();
        router.navigate(['/auth']);
        return throwError(() => refreshError);
      }),
    );
  }

  return refreshedToken$.pipe(
    take(1),
    switchMap((newToken) => {
      const retriedReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
      return next(retriedReq);
    }),
  );
}
