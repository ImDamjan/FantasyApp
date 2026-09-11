import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { PointsSummary, SquadPlayerPoints } from '../../../../core/models/points.models';
import { PointsService } from '../../../../core/services/points.service';
import { GameweekDeadline } from '../../../../core/models/gameweek.models';
import { GameweekService } from '../../../../core/services/gameweek.service';
import { AppShell } from '../../../../shared/components/app-shell/app-shell';
import { PitchPlayer, PitchView } from '../../../../shared/components/pitch-view/pitch-view';
import { PlayerStatsPopup } from '../../../../shared/components/player-stats-popup/player-stats-popup';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [AppShell, RouterLink, DecimalPipe, PitchView, PlayerStatsPopup],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly pointsService = inject(PointsService);
  private readonly gameweekService = inject(GameweekService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly pointsSummary = signal<PointsSummary | null>(null);
  readonly deadlines = signal<GameweekDeadline[]>([]);
  readonly squadPoints = signal<SquadPlayerPoints[]>([]);
  readonly selectedPlayerId = signal<number | null>(null);
  private readonly now = signal(Date.now());
  private tickHandle: ReturnType<typeof setInterval> | null = null;

  readonly squadPitchPlayers = computed<PitchPlayer[]>(() =>
    this.squadPoints().map((p) => ({
      id: p.playerId,
      webName: p.webName,
      position: p.position,
      teamShortName: p.teamShortName,
      priceMillions: p.priceMillions,
      isStarting: p.isStarting,
      benchOrder: p.isStarting ? null : 0,
      isCaptain: p.isCaptain,
      isViceCaptain: p.isViceCaptain,
      points: p.gameweekPoints,
    })),
  );

  readonly selectedPlayer = computed(
    () => this.squadPoints().find((p) => p.playerId === this.selectedPlayerId()) ?? null,
  );

  readonly nextDeadline = computed<GameweekDeadline | null>(() => this.deadlines()[0] ?? null);
  readonly upcomingDeadlines = computed(() => this.deadlines().slice(1));

  readonly countdown = computed(() => {
    const next = this.nextDeadline();
    if (!next) {
      return null;
    }
    const diffMs = new Date(next.deadlineTime).getTime() - this.now();
    if (diffMs <= 0) {
      return { days: 0, hours: 0, minutes: 0, seconds: 0 };
    }
    const totalSeconds = Math.floor(diffMs / 1000);
    return {
      days: Math.floor(totalSeconds / 86400),
      hours: Math.floor((totalSeconds % 86400) / 3600),
      minutes: Math.floor((totalSeconds % 3600) / 60),
      seconds: totalSeconds % 60,
    };
  });

  ngOnInit(): void {
    this.pointsService.getSummary().subscribe((summary) => this.pointsSummary.set(summary));
    this.gameweekService.getUpcomingDeadlines().subscribe((deadlines) => this.deadlines.set(deadlines));
    this.pointsService.getSquadPoints().subscribe((squadPoints) => this.squadPoints.set(squadPoints));
    this.tickHandle = setInterval(() => this.now.set(Date.now()), 1000);
  }

  onSquadPlayerClick(id: number): void {
    this.selectedPlayerId.set(id);
  }

  ngOnDestroy(): void {
    if (this.tickHandle !== null) {
      clearInterval(this.tickHandle);
    }
  }

  pad(value: number): string {
    return value.toString().padStart(2, '0');
  }

  formatDeadline(iso: string): string {
    return new Date(iso).toLocaleString(undefined, {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  onLogout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
