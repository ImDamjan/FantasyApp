import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface PlayerCardData {
  webName: string;
  teamShortName: string;
  priceMillions: number;
  isCaptain?: boolean;
  isViceCaptain?: boolean;
  isSelected?: boolean;
}

@Component({
  selector: 'app-player-card',
  standalone: true,
  templateUrl: './player-card.html',
  styleUrl: './player-card.scss',
})
export class PlayerCard {
  @Input({ required: true }) player!: PlayerCardData;
  @Output() select = new EventEmitter<void>();
}
