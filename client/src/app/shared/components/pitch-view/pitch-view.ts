import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { PlayerPosition } from '../../../core/models/player.models';
import { PlayerCard } from '../player-card/player-card';
import { PlayerActionMenu } from '../player-action-menu/player-action-menu';

export interface PitchPlayer {
  id: number;
  webName: string;
  position: PlayerPosition;
  teamShortName: string;
  priceMillions: number;
  isStarting: boolean;
  benchOrder: number | null;
  isCaptain: boolean;
  isViceCaptain: boolean;
  points?: number;
  /** Shown as an empty "pick a replacement" slot instead of a shirt (transfers screen: outgoing player). */
  isVacant?: boolean;
}

@Component({
  selector: 'app-pitch-view',
  standalone: true,
  imports: [PlayerCard, PlayerActionMenu],
  templateUrl: './pitch-view.html',
  styleUrl: './pitch-view.scss',
})
export class PitchView {
  @Input() players: PitchPlayer[] = [];
  /** Non-null while waiting for a second click to complete a substitution. */
  @Input() selectedId: number | null = null;
  /** Ids of players that are valid substitution targets for the current `selectedId` candidate. */
  @Input() swapTargetIds: number[] = [];
  @Input() allowRemove = false;
  /** When false, clicks always emit playerClick directly instead of opening the action popup (e.g. the transfers screen, where a click means "transfer this player out"). */
  @Input() useActionMenu = true;

  /** Emitted to perform a substitution: first click sets selectedId (via substitute action), second click on another player completes the swap. */
  @Output() playerClick = new EventEmitter<number>();
  @Output() captainChange = new EventEmitter<number>();
  @Output() viceCaptainChange = new EventEmitter<number>();
  @Output() removePlayer = new EventEmitter<number>();

  readonly menuPlayer = signal<PitchPlayer | null>(null);

  isSwapTarget(id: number): boolean {
    return this.swapTargetIds.includes(id);
  }

  private starting(position: PlayerPosition): PitchPlayer[] {
    return this.players.filter((p) => p.isStarting && p.position === position);
  }

  get goalkeepers(): PitchPlayer[] {
    return this.starting('Goalkeeper');
  }

  get defenders(): PitchPlayer[] {
    return this.starting('Defender');
  }

  get midfielders(): PitchPlayer[] {
    return this.starting('Midfielder');
  }

  get forwards(): PitchPlayer[] {
    return this.starting('Forward');
  }

  get bench(): PitchPlayer[] {
    return this.players
      .filter((p) => !p.isStarting)
      .sort((a, b) => (a.benchOrder ?? 0) - (b.benchOrder ?? 0));
  }

  onClick(id: number): void {
    if (!this.useActionMenu || this.selectedId !== null) {
      // Already mid-substitution: this click picks the swap target directly.
      this.playerClick.emit(id);
      return;
    }
    const player = this.players.find((p) => p.id === id) ?? null;
    this.menuPlayer.set(player);
  }

  closeMenu(): void {
    this.menuPlayer.set(null);
  }

  onCardRemove(id: number): void {
    this.removePlayer.emit(id);
  }

  onMakeCaptain(): void {
    const p = this.menuPlayer();
    if (p) {
      this.captainChange.emit(p.id);
    }
    this.menuPlayer.set(null);
  }

  onMakeViceCaptain(): void {
    const p = this.menuPlayer();
    if (p) {
      this.viceCaptainChange.emit(p.id);
    }
    this.menuPlayer.set(null);
  }

  onSubstitute(): void {
    const p = this.menuPlayer();
    this.menuPlayer.set(null);
    if (p) {
      this.playerClick.emit(p.id);
    }
  }

  onRemove(): void {
    const p = this.menuPlayer();
    this.menuPlayer.set(null);
    if (p) {
      this.removePlayer.emit(p.id);
    }
  }
}
