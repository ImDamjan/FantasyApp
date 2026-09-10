import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { PointsHistoryItem, PointsSummary } from '../models/points.models';

@Injectable({ providedIn: 'root' })
export class PointsService {
  private readonly http = inject(HttpClient);

  getSummary(): Observable<PointsSummary> {
    return this.http.get<PointsSummary>(API_ENDPOINTS.points.summary);
  }

  getHistory(): Observable<PointsHistoryItem[]> {
    return this.http.get<PointsHistoryItem[]>(API_ENDPOINTS.points.history);
  }
}
