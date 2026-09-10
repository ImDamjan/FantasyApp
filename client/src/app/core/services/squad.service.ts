import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import {
  PickSquadRequest,
  SetCaptainRequest,
  Squad,
  UpdateLineupRequest,
} from '../models/squad.models';

@Injectable({ providedIn: 'root' })
export class SquadService {
  private readonly squadSignal = signal<Squad | null>(null);

  readonly squad = this.squadSignal.asReadonly();
  readonly startingXI = computed(() => this.squadSignal()?.players.filter((p) => p.isStarting) ?? []);
  readonly bench = computed(() =>
    (this.squadSignal()?.players.filter((p) => !p.isStarting) ?? []).sort(
      (a, b) => (a.benchOrder ?? 0) - (b.benchOrder ?? 0),
    ),
  );

  constructor(private readonly http: HttpClient) {}

  setSquad(squad: Squad): void {
    this.squadSignal.set(squad);
  }

  loadSquad(): Observable<Squad> {
    return this.http
      .get<Squad>(API_ENDPOINTS.squad.get)
      .pipe(tap((squad) => this.squadSignal.set(squad)));
  }

  pickInitialSquad(request: PickSquadRequest): Observable<Squad> {
    return this.http
      .post<Squad>(API_ENDPOINTS.squad.pick, request)
      .pipe(tap((squad) => this.squadSignal.set(squad)));
  }

  updateLineup(request: UpdateLineupRequest): Observable<Squad> {
    return this.http
      .put<Squad>(API_ENDPOINTS.squad.lineup, request)
      .pipe(tap((squad) => this.squadSignal.set(squad)));
  }

  setCaptain(request: SetCaptainRequest): Observable<Squad> {
    return this.http
      .put<Squad>(API_ENDPOINTS.squad.captain, request)
      .pipe(tap((squad) => this.squadSignal.set(squad)));
  }

  activateChip(chip: string): Observable<Squad> {
    return this.http
      .put<Squad>(API_ENDPOINTS.squad.chip, { chip })
      .pipe(tap((squad) => this.squadSignal.set(squad)));
  }
}
