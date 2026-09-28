import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { PlayerFilters, PlayerListResult } from '../models/player.models';

@Injectable({ providedIn: 'root' })
export class PlayerService {
  constructor(private readonly http: HttpClient) {}

  getPlayers(filters: PlayerFilters): Observable<PlayerListResult> {
    let params = new HttpParams();
    if (filters.position) params = params.set('position', filters.position);
    if (filters.maxPrice != null) params = params.set('maxPrice', filters.maxPrice);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.teamId != null) params = params.set('teamId', filters.teamId);
    params = params.set('page', filters.page ?? 1);
    params = params.set('pageSize', filters.pageSize ?? 50);

    return this.http.get<PlayerListResult>(API_ENDPOINTS.players.list, { params });
  }
}
