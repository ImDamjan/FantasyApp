import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LeagueStandings } from '../../../../core/models/league.models';
import { LeagueService } from '../../../../core/services/league.service';

@Component({
  selector: 'app-league-standings-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './league-standings-page.html',
  styleUrl: './league-standings-page.scss',
})
export class LeagueStandingsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly leagueService = inject(LeagueService);

  readonly standings = signal<LeagueStandings | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.leagueService.getStandings(id).subscribe({
      next: (standings) => {
        this.standings.set(standings);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not load standings.');
      },
    });
  }
}
