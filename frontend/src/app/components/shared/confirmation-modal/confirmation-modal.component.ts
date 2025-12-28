import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { ConfirmationModalService, ConfirmationState } from '../../../services/confirmation-modal.service';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-overlay" *ngIf="state.isOpen" (click)="onOverlayClick($event)">
      <div class="modal-container" role="dialog" aria-modal="true">
        <div class="modal-header" [class.modal-header-danger]="state.config?.type === 'danger'" [class.modal-header-warning]="state.config?.type === 'warning'">
          <h3 class="modal-title">{{ state.config?.title }}</h3>
        </div>
        <div class="modal-body">
          <p class="modal-message">{{ state.config?.message }}</p>
        </div>
        <div class="modal-footer">
          <button
            class="btn btn-cancel"
            (click)="onCancel()"
          >
            {{ state.config?.cancelText }}
          </button>
          <button
            class="btn"
            [class.btn-danger]="state.config?.type === 'danger'"
            [class.btn-warning]="state.config?.type === 'warning'"
            [class.btn-primary]="state.config?.type === 'info'"
            (click)="onConfirm()"
          >
            {{ state.config?.confirmText }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10001;
      animation: fadeIn 0.2s ease-out;
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
      }
      to {
        opacity: 1;
      }
    }

    .modal-container {
      background: white;
      border-radius: 12px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      max-width: 450px;
      width: 90%;
      animation: slideUp 0.3s ease-out;
      overflow: hidden;
    }

    @keyframes slideUp {
      from {
        transform: translateY(20px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .modal-header {
      padding: 20px 24px;
      background: #f8f9fa;
      border-bottom: 1px solid #e9ecef;
    }

    .modal-header-danger {
      background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
      border-bottom-color: #f5c6cb;
    }

    .modal-header-warning {
      background: linear-gradient(135deg, #fff3cd 0%, #ffeeba 100%);
      border-bottom-color: #ffeeba;
    }

    .modal-title {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      color: #333;
    }

    .modal-body {
      padding: 24px;
    }

    .modal-message {
      margin: 0;
      font-size: 1rem;
      color: #555;
      line-height: 1.6;
    }

    .modal-footer {
      padding: 16px 24px;
      background: #f8f9fa;
      border-top: 1px solid #e9ecef;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 10px 20px;
      border-radius: 6px;
      font-size: 0.95rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
      border: none;
    }

    .btn-cancel {
      background: #e9ecef;
      color: #495057;
    }

    .btn-cancel:hover {
      background: #dee2e6;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background: #0056b3;
    }

    .btn-danger {
      background: #dc3545;
      color: white;
    }

    .btn-danger:hover {
      background: #c82333;
    }

    .btn-warning {
      background: #ffc107;
      color: #212529;
    }

    .btn-warning:hover {
      background: #e0a800;
    }

    @media (max-width: 576px) {
      .modal-container {
        width: 95%;
        margin: 10px;
      }

      .modal-footer {
        flex-direction: column-reverse;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class ConfirmationModalComponent implements OnInit, OnDestroy {
  state: ConfirmationState = { isOpen: false, config: null };
  private destroy$ = new Subject<void>();

  constructor(private confirmationService: ConfirmationModalService) {}

  ngOnInit(): void {
    this.confirmationService.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.state = state;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onConfirm(): void {
    this.confirmationService.respond(true);
  }

  onCancel(): void {
    this.confirmationService.respond(false);
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }
}
