import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface ConfirmationConfig {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmButtonClass?: string;
  type?: 'danger' | 'warning' | 'info';
}

export interface ConfirmationState {
  isOpen: boolean;
  config: ConfirmationConfig | null;
}

@Injectable({
  providedIn: 'root'
})
export class ConfirmationModalService {
  private stateSubject = new BehaviorSubject<ConfirmationState>({
    isOpen: false,
    config: null
  });

  private responseSubject = new Subject<boolean>();

  state$: Observable<ConfirmationState> = this.stateSubject.asObservable();

  constructor() {}

  confirm(config: ConfirmationConfig): Promise<boolean> {
    const fullConfig: ConfirmationConfig = {
      confirmText: 'Confirm',
      cancelText: 'Cancel',
      confirmButtonClass: 'btn-primary',
      type: 'info',
      ...config
    };

    // Set button class based on type if not explicitly provided
    if (!config.confirmButtonClass) {
      switch (fullConfig.type) {
        case 'danger':
          fullConfig.confirmButtonClass = 'btn-danger';
          break;
        case 'warning':
          fullConfig.confirmButtonClass = 'btn-warning';
          break;
        default:
          fullConfig.confirmButtonClass = 'btn-primary';
      }
    }

    this.stateSubject.next({
      isOpen: true,
      config: fullConfig
    });

    return new Promise<boolean>((resolve) => {
      const subscription = this.responseSubject.subscribe((result) => {
        subscription.unsubscribe();
        resolve(result);
      });
    });
  }

  confirmDelete(itemName: string): Promise<boolean> {
    return this.confirm({
      title: 'Confirm Delete',
      message: `Are you sure you want to delete "${itemName}"? This action cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      type: 'danger'
    });
  }

  confirmAction(title: string, message: string): Promise<boolean> {
    return this.confirm({
      title,
      message,
      confirmText: 'Yes',
      cancelText: 'No',
      type: 'warning'
    });
  }

  respond(result: boolean): void {
    this.responseSubject.next(result);
    this.close();
  }

  close(): void {
    this.stateSubject.next({
      isOpen: false,
      config: null
    });
  }
}
