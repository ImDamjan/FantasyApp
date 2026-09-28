import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ShirtIcon } from '../shirt-icon/shirt-icon';

export interface PlayerCardData {
  webName: string;
  teamShortName: string;
  priceMillions: number;
  isCaptain?: boolean;
  isViceCaptain?: boolean;
  isSelected?: boolean;
  isSwapTarget?: boolean;
  removable?: boolean;
  points?: number;
}

@Component({
  selector: 'app-player-card',
  standalone: true,
  imports: [ShirtIcon],
  templateUrl: './player-card.html',
  styleUrl: './player-card.scss',
})
export class PlayerCard {
  @Input({ required: true }) player!: PlayerCardData;
  @Output() select = new EventEmitter<void>();
  @Output() remove = new EventEmitter<void>();
}
