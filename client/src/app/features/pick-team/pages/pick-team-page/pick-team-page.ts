import { Component, OnInit, WritableSignal, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PlayerListItem, PlayerPosition } from '../../../../core/models/player.models';
import { AuthService } from '../../../../core/services/auth.service';
import { PlayerService } from '../../../../core/services/player.service';
import { SquadService } from '../../../../core/services/squad.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AppShell } from '../../../../shared/components/app-shell/app-shell';
import { PitchPlayer, PitchView } from '../../../../shared/components/pitch-view/pitch-view';
import { PlayerSearchList } from '../../../../shared/components/player-search-list/player-search-list';
import { SquadBuildPitch } from '../../../../shared/components/squad-build-pitch/squad-build-pitch';

type ChipKind = 'TripleCaptain' | 'BenchBoost' | 'WildCard';

const CHIP_LABELS: Record<ChipKind, string> = {
  TripleCaptain: 'Triple Captain',
  BenchBoost: 'Bench Boost',
  WildCard: 'Wild Card',
};

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
  imports: [AppShell, PitchView, PlayerSearchList, SquadBuildPitch],
  templateUrl: './pick-team-page.html',
  styleUrl: './pick-team-page.scss',
})
export class PickTeamPage implements OnInit {
  private readonly squadService = inject(SquadService);
  private readonly authService = inject(AuthService);
  private readonly playerService = inject(PlayerService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly squad = this.squadService.squad;
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly autoPicking = signal(false);
  readonly pendingChip = signal<ChipKind | null>(null);
  readonly pendingChipLabel = computed(() => {
    const chip = this.pendingChip();
    return chip ? CHIP_LABELS[chip] : '';
  });

  // "build a squad from scratch" mode (no squad picked yet)
  readonly buildPlayers = signal<BuildPlayer[]>([]);
  readonly buildStep = signal<'select' | 'lineup'>('select');
  readonly buildCaptainId = signal<number | null>(null);
  readonly buildViceCaptainId = signal<number | null>(null);
  readonly buildSwapCandidateId = signal<number | null>(null);
  /** Non-null while the "add player to this empty slot" search overlay is open. */
  readonly slotSearchPosition = signal<PlayerPosition | null>(null);

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
  readonly buildSwapTargetIds = computed(() =>
    this.findSwapTargetIds(this.buildPlayers(), this.buildSwapCandidateId()),
  );

  // "edit an existing squad's lineup" mode
  readonly lineupPlayers = signal<BuildPlayer[]>([]);
  readonly lineupCaptainId = signal<number | null>(null);
  readonly lineupViceCaptainId = signal<number | null>(null);
  readonly lineupSwapCandidateId = signal<number | null>(null);
  readonly lineupPitchPlayers = computed(() =>
    this.toPitchPlayers(this.lineupPlayers(), this.lineupCaptainId(), this.lineupViceCaptainId()),
  );
  readonly lineupSwapTargetIds = computed(() =>
    this.findSwapTargetIds(this.lineupPlayers(), this.lineupSwapCandidateId()),
  );

  /** Snapshot of the last-saved lineup state, used to show the Save button only when something changed. */
  private lineupSnapshot = '';
  /** Deep copy of the last-saved lineup, restored verbatim by "Discard changes". */
  private lineupSavedPlayers: BuildPlayer[] = [];
  private lineupSavedCaptainId: number | null = null;
  private lineupSavedViceCaptainId: number | null = null;
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
    this.captureLineupSavedState();
  }

  /** Records the current lineup signals as the "last-saved" state that Discard changes reverts to. */
  private captureLineupSavedState(): void {
    this.lineupSnapshot = this.snapshotLineup(this.lineupPlayers(), this.lineupCaptainId(), this.lineupViceCaptainId());
    this.lineupSavedPlayers = this.lineupPlayers().map((p) => ({ ...p }));
    this.lineupSavedCaptainId = this.lineupCaptainId();
    this.lineupSavedViceCaptainId = this.lineupViceCaptainId();
  }

  discardLineupChanges(): void {
    this.lineupPlayers.set(this.lineupSavedPlayers.map((p) => ({ ...p })));
    this.lineupCaptainId.set(this.lineupSavedCaptainId);
    this.lineupViceCaptainId.set(this.lineupSavedViceCaptainId);
    this.lineupSwapCandidateId.set(null);
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
      this.toastService.error(`You already have enough ${player.position.toLowerCase()}s.`);
      return;
    }
    if (current.filter((p) => p.teamId === player.teamId).length >= 3) {
      this.toastService.error('You cannot pick more than 3 players from the same club.');
      return;
    }
    if (this.buildBudgetSpent() + player.priceMillions > 100) {
      this.toastService.error('That would exceed your £100m budget.');
      return;
    }

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

  openSlotSearch(position: PlayerPosition): void {
    this.slotSearchPosition.set(position);
  }

  closeSlotSearch(): void {
    this.slotSearchPosition.set(null);
  }

  onSlotAddPlayer(player: PlayerListItem): void {
    const before = this.buildPlayers().length;
    this.onAddBuildPlayer(player);
    if (this.buildPlayers().length > before) {
      this.closeSlotSearch();
    }
  }

  /** Randomly fills all 15 squad slots, spends nearly the full £100m budget, picks a starting XI and a random captain/vice-captain. */
  autoPick(): void {
    this.autoPicking.set(true);

    const positions: PlayerPosition[] = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];
    forkJoin(positions.map((position) => this.playerService.getPlayers({ position, pageSize: 200 }))).subscribe({
      next: ([goalkeepers, defenders, midfielders, forwards]) => {
        this.autoPicking.set(false);

        const pools: Record<PlayerPosition, PlayerListItem[]> = {
          Goalkeeper: goalkeepers.players.filter((p) => p.status === 'a'),
          Defender: defenders.players.filter((p) => p.status === 'a'),
          Midfielder: midfielders.players.filter((p) => p.status === 'a'),
          Forward: forwards.players.filter((p) => p.status === 'a'),
        };

        const squad = this.buildAutoSquad(pools);
        if (!squad) {
          this.toastService.error('Could not auto pick a squad. Try again.');
          return;
        }

        const byPos = (pos: PlayerPosition) => squad.filter((p) => p.position === pos);
        const starters = new Set<number>([
          ...byPos('Goalkeeper').slice(0, 1).map((p) => p.id),
          ...byPos('Defender').slice(0, 4).map((p) => p.id),
          ...byPos('Midfielder').slice(0, 4).map((p) => p.id),
          ...byPos('Forward').slice(0, 2).map((p) => p.id),
        ]);
        const withLineup = squad.map((p) => ({ ...p, isStarting: starters.has(p.id) }));
        this.reassignBenchOrder(withLineup);

        const shuffledStarters = withLineup
          .filter((p) => p.isStarting)
          .map((p) => p.id)
          .sort(() => Math.random() - 0.5);
        this.buildCaptainId.set(shuffledStarters[0] ?? null);
        this.buildViceCaptainId.set(shuffledStarters[1] ?? null);

        this.buildPlayers.set(withLineup);
        this.buildStep.set('lineup');
      },
      error: () => {
        this.autoPicking.set(false);
        this.toastService.error('Could not auto pick a squad.');
      },
    });
  }

  /**
   * Randomly assembles a valid 15-player squad (2/5/5/3, max 3 per club, within £100m), then
   * greedily upgrades random slots to pricier alternatives until the budget is nearly exhausted.
   * Retries a few times in case an unlucky shuffle can't satisfy the club-limit constraint.
   */
  private buildAutoSquad(pools: Record<PlayerPosition, PlayerListItem[]>): BuildPlayer[] | null {
    const budget = 100;
    const clubLimit = 3;
    const positions: PlayerPosition[] = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];

    for (let attempt = 0; attempt < 30; attempt++) {
      const clubCounts = new Map<number, number>();
      const selected: PlayerListItem[] = [];
      let ok = true;

      for (const position of positions) {
        const need = POSITION_LIMITS[position];
        const sortedByPrice = [...pools[position]].sort((a, b) => a.priceMillions - b.priceMillions);
        // Bias toward the cheaper 70% of the pool so there's budget room left for the upgrade pass.
        const cheapPoolSize = Math.max(need, Math.floor(sortedByPrice.length * 0.7));
        const candidates = this.shuffle(sortedByPrice.slice(0, cheapPoolSize));

        let picked = 0;
        for (const player of candidates) {
          if (picked >= need) break;
          const clubCount = clubCounts.get(player.teamId) ?? 0;
          if (clubCount >= clubLimit) continue;
          selected.push(player);
          clubCounts.set(player.teamId, clubCount + 1);
          picked++;
        }
        if (picked < need) {
          ok = false;
          break;
        }
      }

      if (!ok) continue;

      const totalCost = selected.reduce((sum, p) => sum + p.priceMillions, 0);
      if (totalCost > budget) continue;

      this.upgradeTowardsBudget(selected, pools, clubCounts, budget - totalCost, clubLimit);

      return selected.map((p) => ({
        id: p.id,
        webName: p.webName,
        position: p.position,
        teamId: p.teamId,
        teamShortName: p.teamShortName,
        priceMillions: p.priceMillions,
        isStarting: false,
        benchOrder: null,
      }));
    }

    return null;
  }

  /** Mutates `selected` in place, swapping in pricier alternatives until `remaining` budget is nearly spent. */
  private upgradeTowardsBudget(
    selected: PlayerListItem[],
    pools: Record<PlayerPosition, PlayerListItem[]>,
    clubCounts: Map<number, number>,
    initialRemaining: number,
    clubLimit: number,
  ): void {
    let remaining = Math.round(initialRemaining * 10) / 10;
    const selectedIds = new Set(selected.map((p) => p.id));

    let guard = 0;
    while (remaining > 0.05 && guard < 300) {
      guard++;
      let improved = false;

      for (const idx of this.shuffle(selected.map((_, i) => i))) {
        const current = selected[idx];
        const upgrade = pools[current.position]
          .filter((p) => !selectedIds.has(p.id))
          .filter((p) => p.priceMillions > current.priceMillions)
          .filter((p) => p.priceMillions - current.priceMillions <= remaining + 0.001)
          .filter((p) => {
            const clubCount = clubCounts.get(p.teamId) ?? 0;
            const adjustedClubCount = p.teamId === current.teamId ? clubCount - 1 : clubCount;
            return adjustedClubCount < clubLimit;
          })
          .sort((a, b) => b.priceMillions - a.priceMillions)[0];

        if (!upgrade) continue;

        clubCounts.set(current.teamId, (clubCounts.get(current.teamId) ?? 1) - 1);
        clubCounts.set(upgrade.teamId, (clubCounts.get(upgrade.teamId) ?? 0) + 1);
        selectedIds.delete(current.id);
        selectedIds.add(upgrade.id);
        remaining = Math.round((remaining - (upgrade.priceMillions - current.priceMillions)) * 10) / 10;
        selected[idx] = upgrade;
        improved = true;
        break;
      }

      if (!improved) break;
    }
  }

  private shuffle<T>(arr: T[]): T[] {
    const result = [...arr];
    for (let i = result.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
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
      this.toastService.error('Pick a captain and a vice-captain.');
      return;
    }

    this.saving.set(true);

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
          this.toastService.error(err.error?.message ?? 'Could not save squad.');
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
      this.toastService.error('Pick a captain and a vice-captain.');
      return;
    }

    this.saving.set(true);

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
          this.captureLineupSavedState();
          this.toastService.success('Lineup saved.');
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error(err.error?.message ?? 'Could not save lineup.');
        },
      });
  }

  requestActivateChip(chip: ChipKind): void {
    this.pendingChip.set(chip);
  }

  cancelActivateChip(): void {
    this.pendingChip.set(null);
  }

  confirmActivateChip(): void {
    const chip = this.pendingChip();
    if (!chip) {
      return;
    }
    this.pendingChip.set(null);

    this.saving.set(true);
    this.squadService.activateChip(chip).subscribe({
      next: () => this.saving.set(false),
      error: (err) => {
        this.saving.set(false);
        this.toastService.error(err.error?.message ?? 'Could not activate chip.');
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
      this.toastService.error(formationError);
      return { candidateId: null, swapped: null };
    }

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

  /**
   * Given a swap `candidateId` (or null), returns the ids of every other player that could
   * legally be swapped with it — i.e. is on the opposite side of the starting/bench line and,
   * after the swap, still leaves a valid starting XI (any position can replace any other, as
   * long as the per-position min/max in `STARTING_LIMITS` still holds).
   */
  private findSwapTargetIds(players: BuildPlayer[], candidateId: number | null): number[] {
    if (candidateId === null) {
      return [];
    }
    const candidate = players.find((p) => p.id === candidateId);
    if (!candidate) {
      return [];
    }

    return players
      .filter((p) => p.id !== candidateId && p.isStarting !== candidate.isStarting)
      .filter((p) => {
        const simulated = players.map((sp) => {
          if (sp.id === candidateId) return { ...sp, isStarting: p.isStarting };
          if (sp.id === p.id) return { ...sp, isStarting: candidate.isStarting };
          return sp;
        });
        return this.findFormationError(simulated) === null;
      })
      .map((p) => p.id);
  }
}
