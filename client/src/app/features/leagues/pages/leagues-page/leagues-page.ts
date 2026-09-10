import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LeagueService } from '../../../../core/services/league.service';

@Component({
  selector: 'app-leagues-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './leagues-page.html',
  styleUrl: './leagues-page.scss',
})
export class LeaguesPage implements OnInit {
  private readonly leagueService = inject(LeagueService);

  readonly leagues = this.leagueService.myLeagues;
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly joining = signal(false);
  readonly errorMessage = signal<string | null>(null);

  newLeagueName = '';
  joinCode = '';

  ngOnInit(): void {
    this.leagueService.loadMyLeagues().subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }

  createLeague(): void {
    if (!this.newLeagueName.trim()) {
      return;
    }

    this.creating.set(true);
    this.errorMessage.set(null);

    this.leagueService.createLeague({ name: this.newLeagueName.trim() }).subscribe({
      next: () => {
        this.creating.set(false);
        this.newLeagueName = '';
      },
      error: (err) => {
        this.creating.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not create league.');
      },
    });
  }

  joinLeague(): void {
    if (!this.joinCode.trim()) {
      return;
    }

    this.joining.set(true);
    this.errorMessage.set(null);

    this.leagueService.joinLeague({ joinCode: this.joinCode.trim() }).subscribe({
      next: () => {
        this.joining.set(false);
        this.joinCode = '';
      },
      error: (err) => {
        this.joining.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not join league.');
      },
    });
  }
}
