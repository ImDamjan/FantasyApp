import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SquadPlayerPoints } from '../../../core/models/points.models';
import { ShirtIcon } from '../shirt-icon/shirt-icon';

@Component({
  selector: 'app-player-stats-popup',
  standalone: true,
  imports: [ShirtIcon],
  templateUrl: './player-stats-popup.html',
  styleUrl: './player-stats-popup.scss',
})
export class PlayerStatsPopup {
  @Input({ required: true }) player!: SquadPlayerPoints;
  @Output() close = new EventEmitter<void>();

  get breakdown(): { label: string; value: number }[] {
    const p = this.player;
    return [
      { label: 'Minutes played', value: p.minutes },
      { label: 'Goals scored', value: p.goalsScored },
      { label: 'Assists', value: p.assists },
      { label: 'Clean sheets', value: p.cleanSheets },
      { label: 'Goals conceded', value: p.goalsConceded },
      { label: 'Saves', value: p.saves },
      { label: 'Bonus', value: p.bonus },
      { label: 'Yellow cards', value: p.yellowCards },
      { label: 'Red cards', value: p.redCards },
    ].filter((row) => row.value !== 0);
  }

  difficultyLabel(difficulty: number): string {
    if (difficulty <= 2) return 'easy';
    if (difficulty === 3) return 'medium';
    return 'hard';
  }
}
