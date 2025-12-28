import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Toast {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration: number;
  dismissible: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts: Toast[] = [];
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  private nextId = 0;

  toasts$: Observable<Toast[]> = this.toastsSubject.asObservable();

  constructor() {}

  show(type: Toast['type'], title: string, message: string, duration: number = 5000, dismissible: boolean = true): number {
    const id = this.nextId++;
    const toast: Toast = { id, type, title, message, duration, dismissible };

    this.toasts.push(toast);
    this.toastsSubject.next([...this.toasts]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }

    return id;
  }

  success(title: string, message: string = '', duration: number = 5000): number {
    return this.show('success', title, message, duration);
  }

  error(title: string, message: string = '', duration: number = 7000): number {
    return this.show('error', title, message, duration);
  }

  warning(title: string, message: string = '', duration: number = 5000): number {
    return this.show('warning', title, message, duration);
  }

  info(title: string, message: string = '', duration: number = 5000): number {
    return this.show('info', title, message, duration);
  }

  dismiss(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
    this.toastsSubject.next([...this.toasts]);
  }
}
