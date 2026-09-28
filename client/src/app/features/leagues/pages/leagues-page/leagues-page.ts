import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { LeagueService } from '../../../../core/services/league.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AppShell } from '../../../../shared/components/app-shell/app-shell';

@Component({
  selector: 'app-leagues-page',
  standalone: true,
  imports: [AppShell, FormsModule, RouterLink],
  templateUrl: './leagues-page.html',
  styleUrl: './leagues-page.scss',
})
export class LeaguesPage implements OnInit {
  private readonly leagueService = inject(LeagueService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly leagues = this.leagueService.myLeagues;
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly joining = signal(false);

  newLeagueName = '';
  joinCode = '';

  ngOnInit(): void {
    this.leagueService.loadMyLeagues().subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }

  createLeague(): void {
    const name = this.newLeagueName.trim();
    if (name.length < 3 || name.length > 20) {
      this.toastService.error('League name must be between 3 and 20 characters.');
      return;
    }

    this.creating.set(true);

    this.leagueService.createLeague({ name: this.newLeagueName.trim() }).subscribe({
      next: () => {
        this.creating.set(false);
        this.newLeagueName = '';
      },
      error: (err) => {
        this.creating.set(false);
        this.toastService.error(err.error?.message ?? 'Could not create league.');
      },
    });
  }

  joinLeague(): void {
    if (!this.joinCode.trim()) {
      return;
    }

    this.joining.set(true);

    this.leagueService.joinLeague({ joinCode: this.joinCode.trim() }).subscribe({
      next: () => {
        this.joining.set(false);
        this.joinCode = '';
      },
      error: (err) => {
        this.joining.set(false);
        this.toastService.error(err.error?.message ?? 'Could not join league.');
      },
    });
  }

  copyJoinCode(event: Event, joinCode: string): void {
    event.preventDefault();
    event.stopPropagation();

    navigator.clipboard
      .writeText(joinCode)
      .then(() => this.toastService.success('Join code copied to clipboard.'))
      .catch(() => this.toastService.error('Could not copy join code.'));
  }

  onLogout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
