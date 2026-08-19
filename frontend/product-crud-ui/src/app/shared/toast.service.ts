import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'info';

export interface ToastMessage {
  message: string;
  type: ToastType;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toastSubject = new BehaviorSubject<ToastMessage | null>(null);

  readonly toast$ = this.toastSubject.asObservable();

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  info(message: string): void {
    this.show(message, 'info');
  }

  clear(): void {
    this.toastSubject.next(null);
  }

  private show(message: string, type: ToastType): void {
    this.toastSubject.next({ message, type });

    window.setTimeout(() => {
      this.clear();
    }, 3500);
  }
}
