import { Injectable, signal } from '@angular/core';

export type ToastType = 'error' | 'success';

export interface ToastItem {
  id: number;
  message: string;
  type: ToastType;
  duration: number;
  remaining: number;
  paused: boolean;
}

const DEFAULT_DURATION_MS = 4500;
const TICK_MS = 50;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toastsSignal = signal<ToastItem[]>([]);
  readonly toasts = this.toastsSignal.asReadonly();

  private nextId = 1;
  private tickHandle: ReturnType<typeof setInterval> | null = null;

  show(message: string, type: ToastType = 'error', duration = DEFAULT_DURATION_MS): void {
    const toast: ToastItem = {
      id: this.nextId++,
      message,
      type,
      duration,
      remaining: duration,
      paused: false,
    };
    this.toastsSignal.update((list) => [...list, toast]);
    this.ensureTicking();
  }

  success(message: string, duration = DEFAULT_DURATION_MS): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration = DEFAULT_DURATION_MS): void {
    this.show(message, 'error', duration);
  }

  dismiss(id: number): void {
    this.toastsSignal.update((list) => list.filter((t) => t.id !== id));
  }

  pause(id: number): void {
    this.toastsSignal.update((list) => list.map((t) => (t.id === id ? { ...t, paused: true } : t)));
  }

  resume(id: number): void {
    this.toastsSignal.update((list) => list.map((t) => (t.id === id ? { ...t, paused: false } : t)));
  }

  private ensureTicking(): void {
    if (this.tickHandle !== null) {
      return;
    }
    this.tickHandle = setInterval(() => {
      let anyLeft = false;
      this.toastsSignal.update((list) =>
        list
          .map((t) => (t.paused ? t : { ...t, remaining: t.remaining - TICK_MS }))
          .filter((t) => {
            const keep = t.remaining > 0;
            anyLeft = anyLeft || keep;
            return keep;
          }),
      );
      if (!anyLeft && this.tickHandle !== null) {
        clearInterval(this.tickHandle);
        this.tickHandle = null;
      }
    }, TICK_MS);
  }
}
