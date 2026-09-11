export interface TeamKit {
  /** Shirt body color */
  primary: string;
  /** Second color: stripe color (pattern 'stripes') or trim color (pattern 'solid') */
  secondary: string;
  /** Sleeve color */
  sleeve: string;
  /** Shorts color */
  shorts: string;
  pattern: 'solid' | 'stripes';
}

const DEFAULT_KIT: TeamKit = {
  primary: '#6b7280',
  secondary: '#ffffff',
  sleeve: '#6b7280',
  shorts: '#1f2937',
  pattern: 'solid',
};

/**
 * Real-world home-kit colors for current Premier League clubs, by FPL short_name.
 * Colors only (facts, not club/sponsor artwork) — rendered as generic vector shirts,
 * the same approach the official FPL site itself uses for pitch-view icons.
 */
export const TEAM_KITS: Record<string, TeamKit> = {
  ARS: { primary: '#ef0107', secondary: '#ffffff', sleeve: '#ffffff', shorts: '#ffffff', pattern: 'solid' },
  AVL: { primary: '#670e36', secondary: '#95bfe5', sleeve: '#95bfe5', shorts: '#ffffff', pattern: 'solid' },
  BOU: { primary: '#da291c', secondary: '#000000', sleeve: '#000000', shorts: '#000000', pattern: 'stripes' },
  BRE: { primary: '#e30613', secondary: '#ffffff', sleeve: '#000000', shorts: '#000000', pattern: 'stripes' },
  BHA: { primary: '#0057b8', secondary: '#ffffff', sleeve: '#ffffff', shorts: '#ffffff', pattern: 'stripes' },
  BUR: { primary: '#6c1d45', secondary: '#99d6ea', sleeve: '#99d6ea', shorts: '#ffffff', pattern: 'solid' },
  CHE: { primary: '#034694', secondary: '#ffffff', sleeve: '#034694', shorts: '#034694', pattern: 'solid' },
  CRY: { primary: '#1b458f', secondary: '#c4122e', sleeve: '#c4122e', shorts: '#1b458f', pattern: 'stripes' },
  EVE: { primary: '#003399', secondary: '#ffffff', sleeve: '#003399', shorts: '#ffffff', pattern: 'solid' },
  FUL: { primary: '#ffffff', secondary: '#000000', sleeve: '#ffffff', shorts: '#000000', pattern: 'solid' },
  LEE: { primary: '#ffffff', secondary: '#1d428a', sleeve: '#ffffff', shorts: '#ffffff', pattern: 'solid' },
  LIV: { primary: '#c8102e', secondary: '#c8102e', sleeve: '#c8102e', shorts: '#c8102e', pattern: 'solid' },
  MCI: { primary: '#6cabdd', secondary: '#ffffff', sleeve: '#6cabdd', shorts: '#ffffff', pattern: 'solid' },
  MUN: { primary: '#da291c', secondary: '#ffffff', sleeve: '#da291c', shorts: '#ffffff', pattern: 'solid' },
  NEW: { primary: '#241f20', secondary: '#ffffff', sleeve: '#241f20', shorts: '#241f20', pattern: 'stripes' },
  NFO: { primary: '#dd0000', secondary: '#ffffff', sleeve: '#ffffff', shorts: '#ffffff', pattern: 'solid' },
  SUN: { primary: '#eb172b', secondary: '#ffffff', sleeve: '#000000', shorts: '#000000', pattern: 'stripes' },
  TOT: { primary: '#ffffff', secondary: '#132257', sleeve: '#ffffff', shorts: '#132257', pattern: 'solid' },
  WHU: { primary: '#7a263a', secondary: '#1bb1e7', sleeve: '#1bb1e7', shorts: '#ffffff', pattern: 'solid' },
  WOL: { primary: '#fdb913', secondary: '#231f20', sleeve: '#231f20', shorts: '#231f20', pattern: 'solid' },
};

export function getTeamKit(teamShortName: string): TeamKit {
  return TEAM_KITS[teamShortName] ?? DEFAULT_KIT;
}
