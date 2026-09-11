import { environment } from '../../../environments/environment';

const AUTH_BASE = `${environment.apiUrl}/auth`;
const PLAYERS_BASE = `${environment.apiUrl}/players`;
const SQUAD_BASE = `${environment.apiUrl}/squad`;
const TRANSFERS_BASE = `${environment.apiUrl}/transfers`;
const LEAGUES_BASE = `${environment.apiUrl}/leagues`;
const POINTS_BASE = `${environment.apiUrl}/points`;
const GAMEWEEKS_BASE = `${environment.apiUrl}/gameweeks`;

export const API_ENDPOINTS = {
  auth: {
    register: `${AUTH_BASE}/register`,
    login: `${AUTH_BASE}/login`,
    refreshToken: `${AUTH_BASE}/refresh-token`,
    revokeToken: `${AUTH_BASE}/revoke-token`,
    forgotPassword: `${AUTH_BASE}/forgot-password`,
    resetPassword: `${AUTH_BASE}/reset-password`,
  },
  players: {
    list: PLAYERS_BASE,
    detail: (id: number) => `${PLAYERS_BASE}/${id}`,
  },
  squad: {
    get: SQUAD_BASE,
    pick: SQUAD_BASE,
    lineup: `${SQUAD_BASE}/lineup`,
    captain: `${SQUAD_BASE}/captain`,
    chip: `${SQUAD_BASE}/chip`,
  },
  transfers: {
    submit: TRANSFERS_BASE,
    history: `${TRANSFERS_BASE}/history`,
  },
  leagues: {
    create: LEAGUES_BASE,
    join: `${LEAGUES_BASE}/join`,
    mine: `${LEAGUES_BASE}/mine`,
    standings: (id: number) => `${LEAGUES_BASE}/${id}/standings`,
  },
  points: {
    summary: `${POINTS_BASE}/summary`,
    history: `${POINTS_BASE}/history`,
    squad: `${POINTS_BASE}/squad`,
  },
  gameweeks: {
    deadlines: `${GAMEWEEKS_BASE}/deadlines`,
  },
};
