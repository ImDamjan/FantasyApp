import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LeagueStandings } from '../../../../core/models/league.models';
import { AuthService } from '../../../../core/services/auth.service';
import { LeagueService } from '../../../../core/services/league.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AppShell } from '../../../../shared/components/app-shell/app-shell';

@Component({
  selector: 'app-league-standings-page',
  standalone: true,
  imports: [AppShell, RouterLink],
  templateUrl: './league-standings-page.html',
  styleUrl: './league-standings-page.scss',
})
export class LeagueStandingsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly leagueService = inject(LeagueService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly standings = signal<LeagueStandings | null>(null);
  readonly loading = signal(true);
  readonly loadFailed = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.leagueService.getStandings(id).subscribe({
      next: (standings) => {
        this.standings.set(standings);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.loadFailed.set(true);
        this.toastService.error(err.error?.message ?? 'Could not load standings.');
      },
    });
  }

  onLogout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
