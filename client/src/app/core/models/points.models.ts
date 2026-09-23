export interface PointsSummary {
  teamName: string;
  managerName: string;
  currentGameweekName: string;
  currentGameweekPoints: number;
  totalPoints: number;
  averageGameweekPoints: number;
  highestGameweekPoints: number;
  overallRank: number;
  totalPlayers: number;
}

export interface SquadPlayerFixture {
  opponentShortName: string;
  isHome: boolean;
  difficulty: number;
  kickoffTime: string | null;
}

export interface SquadPlayerPoints {
  playerId: number;
  webName: string;
  position: 'Goalkeeper' | 'Defender' | 'Midfielder' | 'Forward';
  teamShortName: string;
  priceMillions: number;
  isStarting: boolean;
  benchOrder: number | null;
  isCaptain: boolean;
  isViceCaptain: boolean;

  gameweekPoints: number;
  minutes: number;
  goalsScored: number;
  assists: number;
  cleanSheets: number;
  goalsConceded: number;
  saves: number;
  bonus: number;
  yellowCards: number;
  redCards: number;

  form: number;
  nextFixtures: SquadPlayerFixture[];
}

export interface SquadPoints {
  gameweekName: string;
  isScoring: boolean;
  chipUsed: string | null;
  players: SquadPlayerPoints[];
}
