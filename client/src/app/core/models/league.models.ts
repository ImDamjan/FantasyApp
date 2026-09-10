export interface League {
  id: number;
  name: string;
  joinCode: string;
  isOfficial: boolean;
  memberCount: number;
}

export interface CreateLeagueRequest {
  name: string;
}

export interface JoinLeagueRequest {
  joinCode: string;
}

export interface LeagueStandingEntry {
  userId: number;
  username: string;
  gameweekPoints: number;
  totalPoints: number;
  rank: number;
}

export interface LeagueStandings {
  leagueName: string;
  entries: LeagueStandingEntry[];
}
