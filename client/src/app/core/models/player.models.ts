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

export interface UpcomingFixture {
  opponentShortName: string;
  isHome: boolean;
  difficulty: number;
  kickoffTime: string | null;
}

export interface PlayerDetail {
  id: number;
  firstName: string;
  secondName: string;
  webName: string;
  position: PlayerPosition;
  teamId: number;
  teamName: string;
  teamShortName: string;
  priceMillions: number;
  totalPoints: number;
  form: number;
  averagePoints: number;
  status: string;
  nextFixtures: UpcomingFixture[];
}

export interface PlayerFilters {
  position?: PlayerPosition;
  maxPrice?: number;
  search?: string;
  teamId?: number;
  page?: number;
  pageSize?: number;
}
