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
  @Output() addPlayer = new EventEmitter<PlayerListItem>();

  private readonly playerService = inject(PlayerService);
  private readonly searchTerm$ = new Subject<string>();

  readonly positions: PlayerPosition[] = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];

  searchTerm = '';
  position: PlayerPosition | '' = '';
  maxPrice: number | null = null;

  readonly players = signal<PlayerListItem[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
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

  private fetch() {
    return this.playerService.getPlayers({
      search: this.searchTerm || undefined,
      position: this.position || undefined,
      maxPrice: this.maxPrice ?? undefined,
      pageSize: 30,
    });
  }
}
