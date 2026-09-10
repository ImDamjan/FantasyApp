import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { PointsSummary } from '../../../../core/models/points.models';
import { PointsService } from '../../../../core/services/points.service';
import { Logo } from '../../../../shared/components/logo/logo';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [Logo, RouterLink],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly pointsService = inject(PointsService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly pointsSummary = signal<PointsSummary | null>(null);

  ngOnInit(): void {
    this.pointsService.getSummary().subscribe((summary) => this.pointsSummary.set(summary));
  }

  onLogout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/auth']));
  }
}
