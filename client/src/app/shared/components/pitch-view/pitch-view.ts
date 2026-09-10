import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PlayerPosition } from '../../../core/models/player.models';
import { PlayerCard } from '../player-card/player-card';

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
}

@Component({
  selector: 'app-pitch-view',
  standalone: true,
  imports: [PlayerCard],
  templateUrl: './pitch-view.html',
  styleUrl: './pitch-view.scss',
})
export class PitchView {
  @Input() players: PitchPlayer[] = [];
  @Input() selectedId: number | null = null;
  @Output() playerClick = new EventEmitter<number>();

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
    this.playerClick.emit(id);
  }
}
