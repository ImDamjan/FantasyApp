import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { PlayerService } from '../../../core/services/player.service';
import { PlayerListItem, PlayerPosition } from '../../../core/models/player.models';

@Component({
  selector: 'app-player-search-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './player-search-list.html',
  styleUrl: './player-search-list.scss',
})
export class PlayerSearchList implements OnInit {
  @Input() excludeIds: number[] = [];
  @Input() disableAdd = false;
  /** Ids that should render a red remove (×) button instead of the disabled ✓ (e.g. players already in the squad, on the transfers screen). */
  @Input() removableIds: number[] = [];
  /** When set, pins the position filter to this value and hides the position dropdown (e.g. adding a player into a specific empty pitch slot). */
  @Input() lockedPosition: PlayerPosition | null = null;
  @Output() addPlayer = new EventEmitter<PlayerListItem>();
  @Output() removePlayer = new EventEmitter<number>();

  private readonly playerService = inject(PlayerService);
  private readonly searchTerm$ = new Subject<string>();

  readonly positions: PlayerPosition[] = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];

  searchTerm = '';
  position: PlayerPosition | '' = '';
  maxPrice: number | null = null;

  readonly players = signal<PlayerListItem[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    if (this.lockedPosition) {
      this.position = this.lockedPosition;
    }

    this.searchTerm$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(() => this.fetch()),
      )
      .subscribe((result) => {
        this.players.set(result.players);
        this.loading.set(false);
      });

    this.search();
  }

  onSearchInput(): void {
    this.loading.set(true);
    this.searchTerm$.next(this.searchTerm);
  }

  search(): void {
    this.loading.set(true);
    this.fetch().subscribe((result) => {
      this.players.set(result.players);
      this.loading.set(false);
    });
  }

  isExcluded(id: number): boolean {
    return this.excludeIds.includes(id);
  }

  isRemovable(id: number): boolean {
    return this.removableIds.includes(id);
  }

  private fetch() {
    return this.playerService.getPlayers({
      search: this.searchTerm || undefined,
      position: this.position || undefined,
      maxPrice: this.maxPrice ?? undefined,
      pageSize: 30,
    });
  }
}
