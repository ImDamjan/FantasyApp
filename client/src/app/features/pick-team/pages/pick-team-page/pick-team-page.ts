import { Component, OnInit, WritableSignal, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PlayerListItem, PlayerPosition } from '../../../../core/models/player.models';
import { AuthService } from '../../../../core/services/auth.service';
import { SquadService } from '../../../../core/services/squad.service';
import { AppShell } from '../../../../shared/components/app-shell/app-shell';
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

/** Starting-XI formation limits (min/max players of each position allowed on the pitch). */
const STARTING_LIMITS: Record<PlayerPosition, { min: number; max: number }> = {
  Goalkeeper: { min: 1, max: 1 },
  Defender: { min: 3, max: 5 },
  Midfielder: { min: 3, max: 5 },
  Forward: { min: 1, max: 3 },
};

@Component({
  selector: 'app-pick-team-page',
  standalone: true,
  imports: [AppShell, PitchView, PlayerSearchList],
  templateUrl: './pick-team-page.html',
  styleUrl: './pick-team-page.scss',
})
export class PickTeamPage implements OnInit {
  private readonly squadService = inject(SquadService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

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

  /** Snapshot of the last-saved lineup state, used to show the Save button only when something changed. */
  private lineupSnapshot = '';
  readonly lineupDirty = computed(
    () => this.snapshotLineup(this.lineupPlayers(), this.lineupCaptainId(), this.lineupViceCaptainId()) !== this.lineupSnapshot,
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
    this.lineupSnapshot = this.snapshotLineup(this.lineupPlayers(), this.lineupCaptainId(), this.lineupViceCaptainId());
  }

  private snapshotLineup(players: BuildPlayer[], captainId: number | null, viceCaptainId: number | null): string {
    const starting = players
      .filter((p) => p.isStarting)
      .map((p) => p.id)
      .sort((a, b) => a - b);
    return JSON.stringify({ starting, captainId, viceCaptainId });
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
    const { candidateId, swapped } = this.trySwap(players, this.buildSwapCandidateId(), id);
    this.buildPlayers.set(players);
    this.buildSwapCandidateId.set(candidateId);
    if (swapped) {
      this.transferArmbands(players, swapped, this.buildCaptainId, this.buildViceCaptainId);
    }
  }

  onBuildRemovePlayer(id: number): void {
    this.removeBuildPlayer(id);
    this.buildStep.set('select');
  }

  onBuildMakeCaptain(id: number): void {
    this.setCaptain(this.buildCaptainId, this.buildViceCaptainId, id);
  }

  onBuildMakeViceCaptain(id: number): void {
    this.setViceCaptain(this.buildCaptainId, this.buildViceCaptainId, id);
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
    const { candidateId, swapped } = this.trySwap(players, this.lineupSwapCandidateId(), id);
    this.lineupPlayers.set(players);
    this.lineupSwapCandidateId.set(candidateId);
    if (swapped) {
      this.transferArmbands(players, swapped, this.lineupCaptainId, this.lineupViceCaptainId);
    }
  }

  onLineupMakeCaptain(id: number): void {
    this.setCaptain(this.lineupCaptainId, this.lineupViceCaptainId, id);
  }

  onLineupMakeViceCaptain(id: number): void {
    this.setViceCaptain(this.lineupCaptainId, this.lineupViceCaptainId, id);
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
        next: () => {
          this.saving.set(false);
          this.lineupSnapshot = this.snapshotLineup(
            this.lineupPlayers(),
            this.lineupCaptainId(),
            this.lineupViceCaptainId(),
          );
        },
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

  onLogout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
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

  /**
   * Mutates `players` in place (swapping starting/bench) and returns the new swap-candidate id,
   * plus the [candidateId, clickedId] pair if a swap actually happened (for armband transfer).
   */
  private trySwap(
    players: BuildPlayer[],
    candidateId: number | null,
    clickedId: number,
  ): { candidateId: number | null; swapped: [number, number] | null } {
    if (candidateId === null) {
      return { candidateId: clickedId, swapped: null };
    }
    if (candidateId === clickedId) {
      return { candidateId: null, swapped: null };
    }

    const a = players.find((p) => p.id === candidateId);
    const b = players.find((p) => p.id === clickedId);
    if (!a || !b || a.isStarting === b.isStarting) {
      return { candidateId: clickedId, swapped: null };
    }

    [a.isStarting, b.isStarting] = [b.isStarting, a.isStarting];

    const formationError = this.findFormationError(players);
    if (formationError) {
      // Revert: the swap would break the required formation.
      [a.isStarting, b.isStarting] = [b.isStarting, a.isStarting];
      this.errorMessage.set(formationError);
      return { candidateId: null, swapped: null };
    }

    this.errorMessage.set(null);
    this.reassignBenchOrder(players);
    return { candidateId: null, swapped: [a.id, b.id] };
  }

  /** Whoever just got benched hands their captain/vice-captain armband to whoever just took their starting spot. */
  private transferArmbands(
    players: BuildPlayer[],
    [aId, bId]: [number, number],
    captainId: WritableSignal<number | null>,
    viceCaptainId: WritableSignal<number | null>,
  ): void {
    const a = players.find((p) => p.id === aId);
    const b = players.find((p) => p.id === bId);
    if (!a || !b) {
      return;
    }
    const benched = a.isStarting ? b : a;
    const starting = a.isStarting ? a : b;

    let newCaptainId = captainId();
    let newViceCaptainId = viceCaptainId();
    if (newCaptainId === benched.id) {
      newCaptainId = starting.id;
    }
    if (newViceCaptainId === benched.id) {
      newViceCaptainId = starting.id;
    }
    if (newCaptainId !== null && newCaptainId === newViceCaptainId) {
      // Both armbands landed on the same incoming player (the benched pair held captain AND vice
      // between them) — a player can't hold both, so the vice-captain slot is cleared.
      newViceCaptainId = null;
    }
    captainId.set(newCaptainId);
    viceCaptainId.set(newViceCaptainId);
  }

  /** A player can't be both captain and vice-captain — promoting the vice-captain rotates the old captain into the vice slot. */
  private setCaptain(
    captainId: WritableSignal<number | null>,
    viceCaptainId: WritableSignal<number | null>,
    playerId: number,
  ): void {
    if (viceCaptainId() === playerId) {
      viceCaptainId.set(captainId());
    }
    captainId.set(playerId);
  }

  private setViceCaptain(
    captainId: WritableSignal<number | null>,
    viceCaptainId: WritableSignal<number | null>,
    playerId: number,
  ): void {
    if (captainId() === playerId) {
      captainId.set(viceCaptainId());
    }
    viceCaptainId.set(playerId);
  }

  private reassignBenchOrder(players: BuildPlayer[]): void {
    const bench = players.filter((p) => !p.isStarting);
    bench.forEach((p, i) => (p.benchOrder = i));
    players.filter((p) => p.isStarting).forEach((p) => (p.benchOrder = null));
  }

  /** Returns an error message if the starting XI among `players` violates formation limits, otherwise null. */
  private findFormationError(players: BuildPlayer[]): string | null {
    const starters = players.filter((p) => p.isStarting);
    for (const position of Object.keys(STARTING_LIMITS) as PlayerPosition[]) {
      const count = starters.filter((p) => p.position === position).length;
      const { min, max } = STARTING_LIMITS[position];
      if (count > max) {
        return `You can only have ${max} ${position.toLowerCase()}${max === 1 ? '' : 's'} in your starting XI.`;
      }
      if (count < min) {
        return `You need at least ${min} ${position.toLowerCase()}${min === 1 ? '' : 's'} in your starting XI.`;
      }
    }
    return null;
  }
}
