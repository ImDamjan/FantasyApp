import { Squad } from './squad.models';

export interface TransferItem {
  playerOutId: number;
  playerInId: number;
}

export interface SubmitTransfersRequest {
  transfers: TransferItem[];
}

export interface TransferResult {
  squad: Squad;
  transfersMade: number;
  freeTransfersUsed: number;
  paidTransfers: number;
  pointsCost: number;
}

export interface TransferHistoryItem {
  gameweekName: string;
  playerOutName: string;
  playerInName: string;
  wasFreeTransfer: boolean;
  createdAt: string;
}
