import { Component, inject } from '@angular/core';
import { ToastItem, ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.scss',
})
export class ToastContainer {
  private readonly toastService = inject(ToastService);

  readonly toasts = this.toastService.toasts;

  percent(toast: ToastItem): number {
    return Math.max(0, Math.min(100, (toast.remaining / toast.duration) * 100));
  }

  onMouseEnter(id: number): void {
    this.toastService.pause(id);
  }

  onMouseLeave(id: number): void {
    this.toastService.resume(id);
  }

  close(id: number): void {
    this.toastService.dismiss(id);
  }
}
