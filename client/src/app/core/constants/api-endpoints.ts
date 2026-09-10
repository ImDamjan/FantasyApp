import { environment } from '../../../environments/environment';

const AUTH_BASE = `${environment.apiUrl}/auth`;

export const API_ENDPOINTS = {
  auth: {
    register: `${AUTH_BASE}/register`,
    login: `${AUTH_BASE}/login`,
    refreshToken: `${AUTH_BASE}/refresh-token`,
    revokeToken: `${AUTH_BASE}/revoke-token`,
    forgotPassword: `${AUTH_BASE}/forgot-password`,
    resetPassword: `${AUTH_BASE}/reset-password`,
  },
};
