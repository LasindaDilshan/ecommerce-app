import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { PwaService } from '../../../services/pwa.service';

@Component({
  selector: 'app-online-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="status-banner" *ngIf="!isOnline" [@slideDown]>
      <div class="status-content">
        <span class="status-icon">📡</span>
        <span class="status-text">You're offline. Some features may be limited.</span>
      </div>
    </div>

    <div class="reconnected-toast" *ngIf="showReconnected" [@fadeInOut]>
      <span class="toast-icon">✓</span>
      <span class="toast-text">Back online!</span>
    </div>
  `,
  styles: [`
    .status-banner {
      position: fixed;
      top: 60px;
      left: 0;
      right: 0;
      z-index: 9998;
      animation: slideDown 0.3s ease-out;
    }

    @keyframes slideDown {
      from {
        transform: translateY(-100%);
      }
      to {
        transform: translateY(0);
      }
    }

    .status-content {
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
      color: white;
      padding: 0.75rem 1rem;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.75rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    }

    .status-icon {
      font-size: 1.2rem;
      animation: pulse 2s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% {
        opacity: 1;
      }
      50% {
        opacity: 0.6;
      }
    }

    .status-text {
      font-size: 0.95rem;
      font-weight: 500;
    }

    .reconnected-toast {
      position: fixed;
      top: 80px;
      right: 20px;
      z-index: 9999;
      background: linear-gradient(135deg, #2ecc71 0%, #27ae60 100%);
      color: white;
      padding: 1rem 1.5rem;
      border-radius: 8px;
      display: flex;
      align-items: center;
      gap: 0.75rem;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
      animation: fadeInOut 3s ease-in-out;
    }

    @keyframes fadeInOut {
      0% {
        opacity: 0;
        transform: translateX(100%);
      }
      15%, 85% {
        opacity: 1;
        transform: translateX(0);
      }
      100% {
        opacity: 0;
        transform: translateX(100%);
      }
    }

    .toast-icon {
      font-size: 1.5rem;
      font-weight: bold;
    }

    .toast-text {
      font-size: 1rem;
      font-weight: 600;
    }

    @media (max-width: 768px) {
      .status-banner {
        top: 50px;
      }

      .status-content {
        padding: 0.6rem 0.75rem;
      }

      .status-text {
        font-size: 0.85rem;
      }

      .reconnected-toast {
        top: 70px;
        right: 10px;
        left: 10px;
        padding: 0.75rem 1rem;
      }
    }
  `]
})
export class OnlineStatusComponent implements OnInit, OnDestroy {
  isOnline = true;
  showReconnected = false;
  private destroy$ = new Subject<void>();
  private reconnectedTimeout: any;

  constructor(private pwaService: PwaService) {}

  ngOnInit(): void {
    this.isOnline = this.pwaService.isOnline();

    this.pwaService.watchOnlineStatus()
      .pipe(takeUntil(this.destroy$))
      .subscribe(online => {
        const wasOffline = !this.isOnline;
        this.isOnline = online;

        // Show reconnected toast when coming back online
        if (online && wasOffline) {
          this.showReconnectedToast();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.reconnectedTimeout) {
      clearTimeout(this.reconnectedTimeout);
    }
  }

  private showReconnectedToast(): void {
    this.showReconnected = true;

    if (this.reconnectedTimeout) {
      clearTimeout(this.reconnectedTimeout);
    }

    this.reconnectedTimeout = setTimeout(() => {
      this.showReconnected = false;
    }, 3000);
  }
}
