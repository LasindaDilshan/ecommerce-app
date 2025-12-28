import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { PwaService } from '../../../services/pwa.service';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pwa-prompt" *ngIf="showPrompt" [@slideUp]>
      <div class="prompt-content">
        <button class="close-btn" (click)="dismiss()" aria-label="Close">✕</button>
        <div class="prompt-icon">📱</div>
        <div class="prompt-text">
          <h3>Install Our App</h3>
          <p>Get the best shopping experience! Install our app for faster access, offline browsing, and exclusive features.</p>
        </div>
        <div class="prompt-actions">
          <button class="install-btn" (click)="install()">
            <span class="btn-icon">⬇️</span>
            Install Now
          </button>
          <button class="later-btn" (click)="dismiss()">
            Maybe Later
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .pwa-prompt {
      position: fixed;
      bottom: 20px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 9999;
      max-width: 500px;
      width: calc(100% - 40px);
      animation: slideUp 0.5s ease-out;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateX(-50%) translateY(100%);
      }
      to {
        opacity: 1;
        transform: translateX(-50%) translateY(0);
      }
    }

    .prompt-content {
      background: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
      position: relative;
      border: 2px solid #007bff;
    }

    .close-btn {
      position: absolute;
      top: 1rem;
      right: 1rem;
      background: transparent;
      border: none;
      font-size: 1.5rem;
      color: #999;
      cursor: pointer;
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      transition: all 0.3s;
    }

    .close-btn:hover {
      background: #f5f5f5;
      color: #333;
    }

    .prompt-icon {
      font-size: 3rem;
      text-align: center;
      margin-bottom: 1rem;
    }

    .prompt-text {
      text-align: center;
      margin-bottom: 1.5rem;
    }

    .prompt-text h3 {
      font-size: 1.5rem;
      color: #333;
      margin: 0 0 0.5rem 0;
    }

    .prompt-text p {
      font-size: 1rem;
      color: #666;
      line-height: 1.5;
      margin: 0;
    }

    .prompt-actions {
      display: flex;
      gap: 1rem;
    }

    .install-btn,
    .later-btn {
      flex: 1;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
      border: none;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
    }

    .install-btn {
      background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
      color: white;
    }

    .install-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 16px rgba(0, 123, 255, 0.3);
    }

    .later-btn {
      background: #f5f5f5;
      color: #666;
    }

    .later-btn:hover {
      background: #e0e0e0;
    }

    .btn-icon {
      font-size: 1.2rem;
    }

    @media (max-width: 768px) {
      .pwa-prompt {
        bottom: 10px;
        width: calc(100% - 20px);
      }

      .prompt-content {
        padding: 1rem;
      }

      .prompt-icon {
        font-size: 2.5rem;
      }

      .prompt-text h3 {
        font-size: 1.25rem;
      }

      .prompt-text p {
        font-size: 0.9rem;
      }

      .prompt-actions {
        flex-direction: column;
      }
    }
  `]
})
export class PwaPromptComponent implements OnInit, OnDestroy {
  showPrompt = false;
  private destroy$ = new Subject<void>();
  private readonly DISMISS_KEY = 'pwa-prompt-dismissed';
  private readonly DISMISS_DURATION = 7 * 24 * 60 * 60 * 1000; // 7 days

  constructor(private pwaService: PwaService) {}

  ngOnInit(): void {
    // Check if user previously dismissed the prompt
    if (this.wasDismissedRecently()) {
      return;
    }

    // Wait 30 seconds before showing the prompt
    setTimeout(() => {
      this.pwaService.canInstall$
        .pipe(takeUntil(this.destroy$))
        .subscribe(canInstall => {
          this.showPrompt = canInstall && !this.wasDismissedRecently();
        });
    }, 30000);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async install(): Promise<void> {
    const installed = await this.pwaService.installPwa();
    if (installed) {
      this.showPrompt = false;
    }
  }

  dismiss(): void {
    this.showPrompt = false;
    this.markAsDismissed();
  }

  private wasDismissedRecently(): boolean {
    const dismissedAt = localStorage.getItem(this.DISMISS_KEY);
    if (!dismissedAt) {
      return false;
    }

    const dismissedTime = parseInt(dismissedAt, 10);
    const now = Date.now();
    return (now - dismissedTime) < this.DISMISS_DURATION;
  }

  private markAsDismissed(): void {
    localStorage.setItem(this.DISMISS_KEY, Date.now().toString());
  }
}
