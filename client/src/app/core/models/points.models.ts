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

export interface PointsHistoryItem {
  gameweekName: string;
  rawPoints: number;
  transferCost: number;
  netPoints: number;
  chipUsed: string | null;
  isFinal: boolean;
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
