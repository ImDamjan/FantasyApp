import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PlayerPosition } from '../../../core/models/player.models';
import { PlayerCard } from '../player-card/player-card';

export interface SquadBuildSlotPlayer {
  id: number;
  webName: string;
  position: PlayerPosition;
  teamShortName: string;
  priceMillions: number;
}

interface PositionRow {
  position: PlayerPosition;
  label: string;
  slotCount: number;
}

const POSITION_ROWS: PositionRow[] = [
  { position: 'Goalkeeper', label: 'Goalkeeper', slotCount: 2 },
  { position: 'Defender', label: 'Defender', slotCount: 5 },
  { position: 'Midfielder', label: 'Midfielder', slotCount: 5 },
  { position: 'Forward', label: 'Forward', slotCount: 3 },
];

@Component({
  selector: 'app-squad-build-pitch',
  standalone: true,
  imports: [PlayerCard],
  templateUrl: './squad-build-pitch.html',
  styleUrl: './squad-build-pitch.scss',
})
export class SquadBuildPitch {
  @Input() players: SquadBuildSlotPlayer[] = [];

  @Output() addSlot = new EventEmitter<PlayerPosition>();
  @Output() removePlayer = new EventEmitter<number>();

  readonly rows = POSITION_ROWS;

  slotsFor(row: PositionRow): (SquadBuildSlotPlayer | null)[] {
    const filled = this.players.filter((p) => p.position === row.position);
    const slots: (SquadBuildSlotPlayer | null)[] = [...filled];
    while (slots.length < row.slotCount) {
      slots.push(null);
    }
    return slots;
  }
}
