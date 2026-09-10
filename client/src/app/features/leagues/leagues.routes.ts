import { Routes } from '@angular/router';
import { LeaguesPage } from './pages/leagues-page/leagues-page';
import { LeagueStandingsPage } from './pages/league-standings-page/league-standings-page';

export const LEAGUES_ROUTES: Routes = [
  { path: '', component: LeaguesPage },
  { path: ':id', component: LeagueStandingsPage },
];
