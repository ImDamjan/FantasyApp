import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PlayerListItem } from '../../../../core/models/player.models';
import { SquadService } from '../../../../core/services/squad.service';
import { TransferService } from '../../../../core/services/transfer.service';
import { PitchPlayer, PitchView } from '../../../../shared/components/pitch-view/pitch-view';
import { PlayerSearchList } from '../../../../shared/components/player-search-list/player-search-list';

interface PendingTransfer {
  playerOutId: number;
  playerOutName: string;
  playerOutPrice: number;
  playerInId: number;
  playerInName: string;
  playerInTeamShortName: string;
  playerInPrice: number;
}

@Component({
  selector: 'app-transfers-page',
  standalone: true,
  imports: [RouterLink, PitchView, PlayerSearchList],
  templateUrl: './transfers-page.html',
  styleUrl: './transfers-page.scss',
})
export class TransfersPage implements OnInit {
  private readonly squadService = inject(SquadService);
  private readonly transferService = inject(TransferService);

  readonly squad = this.squadService.squad;
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly selectedOutId = signal<number | null>(null);
  readonly pendingTransfers = signal<PendingTransfer[]>([]);

  readonly pitchPlayers = computed<PitchPlayer[]>(() => {
    const squad = this.squad();
    if (!squad) {
      return [];
    }

    const pending = this.pendingTransfers();
    return squad.players.map((p) => {
      const swap = pending.find((t) => t.playerOutId === p.playerId);
      return {
        id: p.playerId,
        webName: swap ? swap.playerInName : p.webName,
        position: p.position,
        teamShortName: swap ? swap.playerInTeamShortName : p.teamShortName,
        priceMillions: swap ? swap.playerInPrice : p.priceMillions,
        isStarting: p.isStarting,
        benchOrder: p.benchOrder,
        isCaptain: p.isCaptain,
        isViceCaptain: p.isViceCaptain,
      };
    });
  });

  readonly excludeIds = computed(() => {
    const squad = this.squad();
    const ownedIds = squad ? squad.players.map((p) => p.playerId) : [];
    const pendingInIds = this.pendingTransfers().map((t) => t.playerInId);
    return [...ownedIds, ...pendingInIds];
  });

  readonly costDelta = computed(() =>
    this.pendingTransfers().reduce((sum, t) => sum + (t.playerInPrice - t.playerOutPrice), 0),
  );

  readonly projectedBudget = computed(() => {
    const squad = this.squad();
    return squad ? squad.budgetRemainingMillions - this.costDelta() : 0;
  });

  readonly pointsCostPreview = computed(() => {
    const squad = this.squad();
    if (!squad) {
      return 0;
    }
    const extra = Math.max(0, this.pendingTransfers().length - squad.freeTransfersAvailable);
    return extra * 4;
  });

  ngOnInit(): void {
    this.squadService.loadSquad().subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }

  onPitchPlayerClick(id: number): void {
    const alreadyPending = this.pendingTransfers().find((t) => t.playerOutId === id);
    if (alreadyPending) {
      this.pendingTransfers.set(this.pendingTransfers().filter((t) => t.playerOutId !== id));
      if (this.selectedOutId() === id) {
        this.selectedOutId.set(null);
      }
      return;
    }

    this.selectedOutId.set(this.selectedOutId() === id ? null : id);
  }

  onAddReplacement(player: PlayerListItem): void {
    const outId = this.selectedOutId();
    if (outId == null) {
      this.errorMessage.set('Click a player on the pitch first to choose who to replace.');
      return;
    }

    const squad = this.squad();
    const outPlayer = squad?.players.find((p) => p.playerId === outId);
    if (!squad || !outPlayer) {
      return;
    }

    if (outPlayer.position !== player.position) {
      this.errorMessage.set(`Replacement must be a ${outPlayer.position.toLowerCase()}.`);
      return;
    }

    this.errorMessage.set(null);
    this.pendingTransfers.set([
      ...this.pendingTransfers(),
      {
        playerOutId: outId,
        playerOutName: outPlayer.webName,
        playerOutPrice: outPlayer.priceMillions,
        playerInId: player.id,
        playerInName: player.webName,
        playerInTeamShortName: player.teamShortName,
        playerInPrice: player.priceMillions,
      },
    ]);
    this.selectedOutId.set(null);
  }

  removePending(playerOutId: number): void {
    this.pendingTransfers.set(this.pendingTransfers().filter((t) => t.playerOutId !== playerOutId));
  }

  confirmTransfers(): void {
    if (this.pendingTransfers().length === 0) {
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    this.transferService
      .submitTransfers({
        transfers: this.pendingTransfers().map((t) => ({
          playerOutId: t.playerOutId,
          playerInId: t.playerInId,
        })),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.pendingTransfers.set([]);
        },
        error: (err) => {
          this.saving.set(false);
          this.errorMessage.set(err.error?.message ?? 'Could not complete transfers.');
        },
      });
  }
}
