import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { PointsSummary, SquadPoints } from '../models/points.models';

@Injectable({ providedIn: 'root' })
export class PointsService {
  private readonly http = inject(HttpClient);

  getSummary(): Observable<PointsSummary> {
    return this.http.get<PointsSummary>(API_ENDPOINTS.points.summary);
  }

  getSquadPoints(): Observable<SquadPoints> {
    return this.http.get<SquadPoints>(API_ENDPOINTS.points.squad);
  }
}
