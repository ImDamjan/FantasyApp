import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import {
  SubmitTransfersRequest,
  TransferHistoryItem,
  TransferResult,
} from '../models/transfer.models';
import { SquadService } from './squad.service';

@Injectable({ providedIn: 'root' })
export class TransferService {
  private readonly http = inject(HttpClient);
  private readonly squadService = inject(SquadService);

  submitTransfers(request: SubmitTransfersRequest): Observable<TransferResult> {
    return this.http
      .post<TransferResult>(API_ENDPOINTS.transfers.submit, request)
      .pipe(tap((result) => this.squadService.setSquad(result.squad)));
  }

  getHistory(): Observable<TransferHistoryItem[]> {
    return this.http.get<TransferHistoryItem[]>(API_ENDPOINTS.transfers.history);
  }
}
