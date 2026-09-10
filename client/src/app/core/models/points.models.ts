export interface PointsSummary {
  currentGameweekPoints: number;
  totalPoints: number;
}

export interface PointsHistoryItem {
  gameweekName: string;
  rawPoints: number;
  transferCost: number;
  netPoints: number;
  chipUsed: string | null;
  isFinal: boolean;
}
