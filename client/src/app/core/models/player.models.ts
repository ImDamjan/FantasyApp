export type PlayerPosition = 'Goalkeeper' | 'Defender' | 'Midfielder' | 'Forward';

export interface PlayerListItem {
  id: number;
  webName: string;
  position: PlayerPosition;
  teamId: number;
  teamName: string;
  teamShortName: string;
  priceMillions: number;
  totalPoints: number;
  form: number;
  status: string;
}

export interface PlayerListResult {
  players: PlayerListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PlayerFilters {
  position?: PlayerPosition;
  maxPrice?: number;
  search?: string;
  teamId?: number;
  page?: number;
  pageSize?: number;
}
