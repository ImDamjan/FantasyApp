import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PitchPlayer } from '../pitch-view/pitch-view';
import { ShirtIcon } from '../shirt-icon/shirt-icon';

@Component({
  selector: 'app-player-action-menu',
  standalone: true,
  imports: [ShirtIcon],
  templateUrl: './player-action-menu.html',
  styleUrl: './player-action-menu.scss',
})
export class PlayerActionMenu {
  @Input({ required: true }) player!: PitchPlayer;
  @Input() allowRemove = false;

  @Output() makeCaptain = new EventEmitter<void>();
  @Output() makeViceCaptain = new EventEmitter<void>();
  @Output() substitute = new EventEmitter<void>();
  @Output() remove = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();
}
