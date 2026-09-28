import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import {
  CreateLeagueRequest,
  JoinLeagueRequest,
  League,
  LeagueStandings,
} from '../models/league.models';

@Injectable({ providedIn: 'root' })
export class LeagueService {
  private readonly http = inject(HttpClient);

  private readonly myLeaguesSignal = signal<League[]>([]);
  readonly myLeagues = this.myLeaguesSignal.asReadonly();

  loadMyLeagues(): Observable<League[]> {
    return this.http
      .get<League[]>(API_ENDPOINTS.leagues.mine)
      .pipe(tap((leagues) => this.myLeaguesSignal.set(leagues)));
  }

  createLeague(request: CreateLeagueRequest): Observable<League> {
    return this.http
      .post<League>(API_ENDPOINTS.leagues.create, request)
      .pipe(tap((league) => this.myLeaguesSignal.set([...this.myLeaguesSignal(), league])));
  }

  joinLeague(request: JoinLeagueRequest): Observable<League> {
    return this.http
      .post<League>(API_ENDPOINTS.leagues.join, request)
      .pipe(tap((league) => this.myLeaguesSignal.set([...this.myLeaguesSignal(), league])));
  }

  getStandings(leagueId: number): Observable<LeagueStandings> {
    return this.http.get<LeagueStandings>(API_ENDPOINTS.leagues.standings(leagueId));
  }
}
