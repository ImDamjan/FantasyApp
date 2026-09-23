import { PlayerPosition } from './player.models';

export interface SquadPlayer {
  playerId: number;
  webName: string;
  position: PlayerPosition;
  teamId: number;
  teamShortName: string;
  priceMillions: number;
  isStarting: boolean;
  benchOrder: number | null;
  isCaptain: boolean;
  isViceCaptain: boolean;
}

export interface Squad {
  name: string;
  budgetRemainingMillions: number;
  freeTransfersAvailable: number;
  unlimitedTransfers: boolean;
  hasPickedInitialSquad: boolean;
  tripleCaptainUsed: boolean;
  benchBoostUsed: boolean;
  wildCardUsed: boolean;
  activeChip: string | null;
  players: SquadPlayer[];
}

export interface BenchSlot {
  playerId: number;
  order: number;
}

export interface PickSquadRequest {
  playerIds: number[];
  startingPlayerIds: number[];
  captainPlayerId: number;
  viceCaptainPlayerId: number;
  benchOrder: BenchSlot[];
}

export interface UpdateLineupRequest {
  startingPlayerIds: number[];
  captainPlayerId: number;
  viceCaptainPlayerId: number;
  benchOrder: BenchSlot[];
}
