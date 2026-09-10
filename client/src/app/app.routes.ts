import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadChildren: () => import('./features/home/home.routes').then((m) => m.HOME_ROUTES),
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'team',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/pick-team/pick-team.routes').then((m) => m.PICK_TEAM_ROUTES),
  },
  {
    path: 'transfers',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/transfers/transfers.routes').then((m) => m.TRANSFERS_ROUTES),
  },
  {
    path: 'leagues',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/leagues/leagues.routes').then((m) => m.LEAGUES_ROUTES),
  },
  { path: '**', redirectTo: '' },
];
