import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PlayerListItem, PlayerPosition } from '../../../../core/models/player.models';
import { SquadService } from '../../../../core/services/squad.service';
import { PitchPlayer, PitchView } from '../../../../shared/components/pitch-view/pitch-view';
import { PlayerSearchList } from '../../../../shared/components/player-search-list/player-search-list';

interface BuildPlayer {
  id: number;
  webName: string;
  position: PlayerPosition;
  teamId: number;
  teamShortName: string;
  priceMillions: number;
  isStarting: boolean;
  benchOrder: number | null;
}

const POSITION_LIMITS: Record<PlayerPosition, number> = {
  Goalkeeper: 2,
  Defender: 5,
  Midfielder: 5,
  Forward: 3,
};

@Component({
  selector: 'app-pick-team-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PitchView, PlayerSearchList],
  templateUrl: './pick-team-page.html',
  styleUrl: './pick-team-page.scss',
})
export class PickTeamPage implements OnInit {
  private readonly squadService = inject(SquadService);

  readonly squad = this.squadService.squad;
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // "build a squad from scratch" mode (no squad picked yet)
  readonly buildPlayers = signal<BuildPlayer[]>([]);
  readonly buildStep = signal<'select' | 'lineup'>('select');
  readonly buildCaptainId = signal<number | null>(null);
  readonly buildViceCaptainId = signal<number | null>(null);
  readonly buildSwapCandidateId = signal<number | null>(null);

  readonly buildExcludeIds = computed(() => this.buildPlayers().map((p) => p.id));
  readonly buildCounts = computed(() => {
    const counts: Record<PlayerPosition, number> = {
      Goalkeeper: 0,
      Defender: 0,
      Midfielder: 0,
      Forward: 0,
    };
    for (const p of this.buildPlayers()) {
      counts[p.position]++;
    }
    return counts;
  });
  readonly buildBudgetSpent = computed(() =>
    this.buildPlayers().reduce((sum, p) => sum + p.priceMillions, 0),
  );
  readonly canContinueToLineup = computed(() => {
    const counts = this.buildCounts();
    return (
      this.buildPlayers().length === 15 &&
      counts.Goalkeeper === POSITION_LIMITS.Goalkeeper &&
      counts.Defender === POSITION_LIMITS.Defender &&
      counts.Midfielder === POSITION_LIMITS.Midfielder &&
      counts.Forward === POSITION_LIMITS.Forward
    );
  });
  readonly buildPitchPlayers = computed(() =>
    this.toPitchPlayers(this.buildPlayers(), this.buildCaptainId(), this.buildViceCaptainId()),
  );

  // "edit an existing squad's lineup" mode
  readonly lineupPlayers = signal<BuildPlayer[]>([]);
  readonly lineupCaptainId = signal<number | null>(null);
  readonly lineupViceCaptainId = signal<number | null>(null);
  readonly lineupSwapCandidateId = signal<number | null>(null);
  readonly lineupPitchPlayers = computed(() =>
    this.toPitchPlayers(this.lineupPlayers(), this.lineupCaptainId(), this.lineupViceCaptainId()),
  );

  ngOnInit(): void {
    this.squadService.loadSquad().subscribe({
      next: (squad) => {
        this.loading.set(false);
        if (squad.hasPickedInitialSquad) {
          this.initLineupState();
        }
      },
      error: () => this.loading.set(false),
    });
  }

  private initLineupState(): void {
    const squad = this.squad();
    if (!squad) {
      return;
    }

    this.lineupPlayers.set(
      squad.players.map((p) => ({
        id: p.playerId,
        webName: p.webName,
        position: p.position,
        teamId: p.teamId,
        teamShortName: p.teamShortName,
        priceMillions: p.priceMillions,
        isStarting: p.isStarting,
        benchOrder: p.benchOrder,
      })),
    );
    this.lineupCaptainId.set(squad.players.find((p) => p.isCaptain)?.playerId ?? null);
    this.lineupViceCaptainId.set(squad.players.find((p) => p.isViceCaptain)?.playerId ?? null);
  }

  // ---- Build mode: pick 15 players from scratch ----

  onAddBuildPlayer(player: PlayerListItem): void {
    const current = this.buildPlayers();
    if (current.some((p) => p.id === player.id) || current.length >= 15) {
      return;
    }
    if (this.buildCounts()[player.position] >= POSITION_LIMITS[player.position]) {
      this.errorMessage.set(`You already have enough ${player.position.toLowerCase()}s.`);
      return;
    }
    if (current.filter((p) => p.teamId === player.teamId).length >= 3) {
      this.errorMessage.set('You cannot pick more than 3 players from the same club.');
      return;
    }
    if (this.buildBudgetSpent() + player.priceMillions > 100) {
      this.errorMessage.set('That would exceed your £100m budget.');
      return;
    }

    this.errorMessage.set(null);
    this.buildPlayers.set([
      ...current,
      {
        id: player.id,
        webName: player.webName,
        position: player.position,
        teamId: player.teamId,
        teamShortName: player.teamShortName,
        priceMillions: player.priceMillions,
        isStarting: false,
        benchOrder: null,
      },
    ]);
  }

  removeBuildPlayer(id: number): void {
    this.buildPlayers.set(this.buildPlayers().filter((p) => p.id !== id));
  }

  continueToLineup(): void {
    if (!this.canContinueToLineup()) {
      return;
    }

    const players = this.buildPlayers();
    const byPos = (pos: PlayerPosition) => players.filter((p) => p.position === pos);
    const starters = new Set<number>([
      ...byPos('Goalkeeper').slice(0, 1).map((p) => p.id),
      ...byPos('Defender').slice(0, 4).map((p) => p.id),
      ...byPos('Midfielder').slice(0, 4).map((p) => p.id),
      ...byPos('Forward').slice(0, 2).map((p) => p.id),
    ]);

    const updated = players.map((p) => ({ ...p, isStarting: starters.has(p.id) }));
    this.reassignBenchOrder(updated);
    this.buildPlayers.set(updated);
    this.buildStep.set('lineup');
  }

  onBuildPlayerClick(id: number): void {
    const players = [...this.buildPlayers()];
    const candidateId = this.trySwap(players, this.buildSwapCandidateId(), id);
    this.buildPlayers.set(players);
    this.buildSwapCandidateId.set(candidateId);
  }

  submitBuild(): void {
    const captainId = this.buildCaptainId();
    const viceCaptainId = this.buildViceCaptainId();
    if (captainId == null || viceCaptainId == null) {
      this.errorMessage.set('Pick a captain and a vice-captain.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const players = this.buildPlayers();
    this.squadService
      .pickInitialSquad({
        playerIds: players.map((p) => p.id),
        startingPlayerIds: players.filter((p) => p.isStarting).map((p) => p.id),
        captainPlayerId: captainId,
        viceCaptainPlayerId: viceCaptainId,
        benchOrder: players
          .filter((p) => !p.isStarting)
          .map((p) => ({ playerId: p.id, order: p.benchOrder ?? 0 })),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.initLineupState();
        },
        error: (err) => {
          this.saving.set(false);
          this.errorMessage.set(err.error?.message ?? 'Could not save squad.');
        },
      });
  }

  // ---- Lineup mode: edit an already-picked squad ----

  onLineupPlayerClick(id: number): void {
    const players = [...this.lineupPlayers()];
    const candidateId = this.trySwap(players, this.lineupSwapCandidateId(), id);
    this.lineupPlayers.set(players);
    this.lineupSwapCandidateId.set(candidateId);
  }

  saveLineup(): void {
    const captainId = this.lineupCaptainId();
    const viceCaptainId = this.lineupViceCaptainId();
    if (captainId == null || viceCaptainId == null) {
      this.errorMessage.set('Pick a captain and a vice-captain.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const players = this.lineupPlayers();
    this.squadService
      .updateLineup({
        startingPlayerIds: players.filter((p) => p.isStarting).map((p) => p.id),
        captainPlayerId: captainId,
        viceCaptainPlayerId: viceCaptainId,
        benchOrder: players
          .filter((p) => !p.isStarting)
          .map((p) => ({ playerId: p.id, order: p.benchOrder ?? 0 })),
      })
      .subscribe({
        next: () => this.saving.set(false),
        error: (err) => {
          this.saving.set(false);
          this.errorMessage.set(err.error?.message ?? 'Could not save lineup.');
        },
      });
  }

  activateChip(chip: 'TripleCaptain' | 'BenchBoost' | 'WildCard'): void {
    this.saving.set(true);
    this.errorMessage.set(null);

    this.squadService.activateChip(chip).subscribe({
      next: () => this.saving.set(false),
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not activate chip.');
      },
    });
  }

  // ---- Shared helpers ----

  private toPitchPlayers(
    players: BuildPlayer[],
    captainId: number | null,
    viceCaptainId: number | null,
  ): PitchPlayer[] {
    return players.map((p) => ({
      id: p.id,
      webName: p.webName,
      position: p.position,
      teamShortName: p.teamShortName,
      priceMillions: p.priceMillions,
      isStarting: p.isStarting,
      benchOrder: p.benchOrder,
      isCaptain: p.id === captainId,
      isViceCaptain: p.id === viceCaptainId,
    }));
  }

  /** Mutates `players` in place (swapping starting/bench) and returns the new swap-candidate id. */
  private trySwap(players: BuildPlayer[], candidateId: number | null, clickedId: number): number | null {
    if (candidateId === null) {
      return clickedId;
    }
    if (candidateId === clickedId) {
      return null;
    }

    const a = players.find((p) => p.id === candidateId);
    const b = players.find((p) => p.id === clickedId);
    if (!a || !b || a.isStarting === b.isStarting) {
      return clickedId;
    }

    [a.isStarting, b.isStarting] = [b.isStarting, a.isStarting];
    this.reassignBenchOrder(players);
    return null;
  }

  private reassignBenchOrder(players: BuildPlayer[]): void {
    const bench = players.filter((p) => !p.isStarting);
    bench.forEach((p, i) => (p.benchOrder = i));
    players.filter((p) => p.isStarting).forEach((p) => (p.benchOrder = null));
  }
}
