import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { GameweekDeadline } from '../models/gameweek.models';

@Injectable({ providedIn: 'root' })
export class GameweekService {
  private readonly http = inject(HttpClient);

  getUpcomingDeadlines(): Observable<GameweekDeadline[]> {
    return this.http.get<GameweekDeadline[]>(API_ENDPOINTS.gameweeks.deadlines);
  }
}
