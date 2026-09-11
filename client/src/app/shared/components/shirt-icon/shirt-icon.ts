import { Component, Input } from '@angular/core';
import { getTeamKit } from '../../../core/constants/team-kits';

let nextId = 0;

@Component({
  selector: 'app-shirt-icon',
  standalone: true,
  templateUrl: './shirt-icon.html',
  styleUrl: './shirt-icon.scss',
})
export class ShirtIcon {
  @Input({ required: true }) teamShortName!: string;

  readonly patternId = `shirt-stripes-${nextId++}`;

  get kit() {
    return getTeamKit(this.teamShortName);
  }
}
