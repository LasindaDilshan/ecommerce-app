import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TwoFactorService, TwoFactorSetupResponse, TwoFactorStatusResponse } from '../../../services/two-factor.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-two-factor-setup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>Two-Factor Authentication</h1>
        <p class="subtitle">Add an extra layer of security to your account</p>
      </div>

      <!-- Status Card -->
      <div class="status-card" [class.enabled]="status?.isEnabled">
        <div class="status-icon">{{ status?.isEnabled ? '\uD83D\uDD12' : '\uD83D\uDD13' }}</div>
        <div class="status-text">
          <h3>2FA is {{ status?.isEnabled ? 'Enabled' : 'Disabled' }}</h3>
          <p *ngIf="status?.isEnabled">Your account is protected with two-factor authentication</p>
          <p *ngIf="!status?.isEnabled">Enable 2FA to add an extra layer of security</p>
        </div>
        <button *ngIf="!status?.isEnabled && !setupData" (click)="enableTwoFactor()" [disabled]="loading" class="btn btn-primary">
          {{ loading ? 'Setting up...' : 'Enable 2FA' }}
        </button>
        <button *ngIf="status?.isEnabled && !showDisableForm" (click)="showDisableForm = true" class="btn btn-danger">
          Disable 2FA
        </button>
      </div>

      <!-- Setup Flow -->
      <div class="setup-flow" *ngIf="setupData && !status?.isEnabled">
        <div class="step" [class.active]="setupStep === 1">
          <h3>Step 1: Scan QR Code</h3>
          <p>Scan this QR code with your authenticator app (Google Authenticator, Authy, etc.)</p>
          <div class="qr-container">
            <img [src]="setupData.qrCodeUri" alt="QR Code" class="qr-image" *ngIf="setupData.qrCodeUri" />
            <div class="manual-key">
              <p>Can't scan? Enter this key manually:</p>
              <code>{{ setupData.manualEntryKey }}</code>
              <button (click)="copyKey()" class="copy-btn">{{ keyCopied ? 'Copied!' : 'Copy Key' }}</button>
            </div>
          </div>
          <button (click)="setupStep = 2" class="btn btn-primary">Next</button>
        </div>

        <div class="step" [class.active]="setupStep === 2">
          <h3>Step 2: Verify Code</h3>
          <p>Enter the 6-digit code from your authenticator app to verify setup</p>
          <div class="verify-input">
            <input
              type="text"
              [(ngModel)]="verifyCode"
              placeholder="000000"
              maxlength="6"
              class="code-input"
              (keyup.enter)="verifySetup()"
            />
            <button (click)="verifySetup()" [disabled]="verifyCode.length !== 6 || verifying" class="btn btn-primary">
              {{ verifying ? 'Verifying...' : 'Verify & Enable' }}
            </button>
          </div>
          <p *ngIf="verifyError" class="error">{{ verifyError }}</p>
        </div>

        <div class="step" [class.active]="setupStep === 3">
          <h3>Step 3: Save Recovery Codes</h3>
          <p class="warning">Save these recovery codes in a safe place. You'll need them if you lose access to your authenticator app.</p>
          <div class="recovery-codes">
            <div *ngFor="let code of recoveryCodes" class="code-item">{{ code }}</div>
          </div>
          <div class="code-actions">
            <button (click)="copyRecoveryCodes()" class="btn btn-secondary">{{ codesCopied ? 'Copied!' : 'Copy All Codes' }}</button>
            <button (click)="downloadCodes()" class="btn btn-secondary">Download as File</button>
          </div>
          <button (click)="finishSetup()" class="btn btn-primary">I've Saved My Codes</button>
        </div>
      </div>

      <!-- Disable Form -->
      <div class="disable-form" *ngIf="showDisableForm">
        <h3>Disable Two-Factor Authentication</h3>
        <p>Enter a code from your authenticator app to confirm disabling 2FA</p>
        <div class="verify-input">
          <input type="text" [(ngModel)]="disableCode" placeholder="000000" maxlength="6" class="code-input" (keyup.enter)="disableTwoFactor()" />
          <button (click)="disableTwoFactor()" [disabled]="disableCode.length !== 6 || loading" class="btn btn-danger">
            {{ loading ? 'Disabling...' : 'Disable 2FA' }}
          </button>
          <button (click)="showDisableForm = false; disableCode = ''" class="btn btn-secondary">Cancel</button>
        </div>
      </div>

      <!-- Recovery Codes Management -->
      <div class="recovery-section" *ngIf="status?.isEnabled && status?.hasRecoveryCodes">
        <h3>Recovery Codes</h3>
        <p>If you've used or lost your recovery codes, you can regenerate them.</p>
        <div class="verify-input" *ngIf="showRegenForm">
          <input type="text" [(ngModel)]="regenCode" placeholder="Enter 2FA code" maxlength="6" class="code-input" />
          <button (click)="regenerateCodes()" [disabled]="regenCode.length !== 6 || loading" class="btn btn-primary">Regenerate</button>
          <button (click)="showRegenForm = false; regenCode = ''" class="btn btn-secondary">Cancel</button>
        </div>
        <button *ngIf="!showRegenForm" (click)="showRegenForm = true" class="btn btn-secondary">Regenerate Recovery Codes</button>
      </div>
    </div>
  `,
  styles: [`
    .container { max-width: 700px; margin: 0 auto; padding: 20px; }
    .page-header { margin-bottom: 30px; }
    .page-header h1 { color: var(--text-primary); font-size: 2rem; margin: 0 0 8px; }
    .subtitle { color: var(--text-secondary); margin: 0; }
    .status-card { display: flex; align-items: center; gap: 16px; padding: 24px; background: var(--bg-card); border: 2px solid var(--border-color); border-radius: 12px; margin-bottom: 24px; }
    .status-card.enabled { border-color: var(--success); background: rgba(16, 185, 129, 0.05); }
    .status-icon { font-size: 2.5rem; }
    .status-text { flex: 1; }
    .status-text h3 { margin: 0 0 4px; color: var(--text-primary); }
    .status-text p { margin: 0; color: var(--text-secondary); font-size: 0.9rem; }
    .setup-flow { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px; }
    .step { display: none; }
    .step.active { display: block; }
    .step h3 { color: var(--text-primary); margin: 0 0 8px; }
    .step p { color: var(--text-secondary); margin: 0 0 16px; }
    .qr-container { text-align: center; margin-bottom: 20px; }
    .qr-image { width: 200px; height: 200px; border: 4px solid var(--border-color); border-radius: 12px; margin-bottom: 16px; }
    .manual-key { margin-top: 12px; }
    .manual-key code { display: block; padding: 12px; background: var(--bg-secondary); border-radius: 8px; font-size: 1.1rem; letter-spacing: 2px; margin: 8px 0; word-break: break-all; color: var(--text-primary); }
    .copy-btn { padding: 4px 12px; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 6px; cursor: pointer; font-size: 0.85rem; color: var(--text-primary); }
    .verify-input { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; }
    .code-input { padding: 12px 16px; border: 2px solid var(--border-color); border-radius: 8px; font-size: 1.5rem; letter-spacing: 8px; text-align: center; width: 200px; background: var(--bg-secondary); color: var(--text-primary); }
    .code-input:focus { outline: none; border-color: var(--primary); }
    .error { color: var(--danger); font-size: 0.9rem; margin-top: 8px; }
    .warning { color: #b45309; background: #fef3c7; padding: 12px; border-radius: 8px; border: 1px solid #fbbf24; }
    .recovery-codes { display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px; margin: 16px 0; }
    .code-item { padding: 8px 12px; background: var(--bg-secondary); border-radius: 6px; font-family: monospace; font-size: 0.95rem; text-align: center; color: var(--text-primary); }
    .code-actions { display: flex; gap: 8px; margin-bottom: 16px; }
    .disable-form, .recovery-section { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px; margin-bottom: 20px; }
    .disable-form h3, .recovery-section h3 { color: var(--text-primary); margin: 0 0 8px; }
    .disable-form p, .recovery-section p { color: var(--text-secondary); margin: 0 0 16px; font-size: 0.9rem; }
    .btn { padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.95rem; }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-secondary { background: var(--bg-secondary); color: var(--text-primary); border: 1px solid var(--border-color); }
    .btn-danger { background: var(--danger); color: white; }
    @media (max-width: 600px) {
      .status-card { flex-direction: column; text-align: center; }
      .verify-input { flex-direction: column; }
      .code-input { width: 100%; }
      .recovery-codes { grid-template-columns: 1fr; }
    }
  `]
})
export class TwoFactorSetupComponent implements OnInit, OnDestroy {
  status: TwoFactorStatusResponse | null = null;
  setupData: TwoFactorSetupResponse | null = null;
  recoveryCodes: string[] = [];
  setupStep = 1;
  verifyCode = '';
  disableCode = '';
  regenCode = '';
  verifyError = '';
  loading = false;
  verifying = false;
  showDisableForm = false;
  showRegenForm = false;
  keyCopied = false;
  codesCopied = false;
  private destroy$ = new Subject<void>();

  constructor(private twoFactorService: TwoFactorService, private toastService: ToastService) {}

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.twoFactorService.getStatus().pipe(takeUntil(this.destroy$)).subscribe({
      next: (status) => this.status = status,
      error: () => {}
    });
  }

  enableTwoFactor(): void {
    this.loading = true;
    this.twoFactorService.enable().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.setupData = data; this.setupStep = 1; this.loading = false; },
      error: (err) => { this.toastService.error('Error', err.error?.message || 'Failed to enable 2FA'); this.loading = false; }
    });
  }

  verifySetup(): void {
    if (this.verifyCode.length !== 6) return;
    this.verifying = true;
    this.verifyError = '';
    this.twoFactorService.verifySetup(this.verifyCode).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.recoveryCodes = result.recoveryCodes;
        this.setupStep = 3;
        this.verifying = false;
      },
      error: (err) => {
        this.verifyError = err.error?.message || 'Invalid code. Please try again.';
        this.verifying = false;
      }
    });
  }

  finishSetup(): void {
    this.setupData = null;
    this.recoveryCodes = [];
    this.verifyCode = '';
    this.loadStatus();
    this.toastService.success('2FA Enabled', 'Two-factor authentication is now active on your account.');
  }

  disableTwoFactor(): void {
    this.loading = true;
    this.twoFactorService.disable(this.disableCode).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.showDisableForm = false;
        this.disableCode = '';
        this.loading = false;
        this.loadStatus();
        this.toastService.success('2FA Disabled', 'Two-factor authentication has been disabled.');
      },
      error: (err) => {
        this.toastService.error('Error', err.error?.message || 'Invalid code');
        this.loading = false;
      }
    });
  }

  regenerateCodes(): void {
    this.loading = true;
    this.twoFactorService.regenerateRecoveryCodes(this.regenCode).pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.recoveryCodes = result.recoveryCodes;
        this.setupStep = 3;
        this.setupData = {} as TwoFactorSetupResponse; // Show codes step
        this.showRegenForm = false;
        this.regenCode = '';
        this.loading = false;
        this.toastService.success('Codes Regenerated', 'New recovery codes have been generated.');
      },
      error: (err) => {
        this.toastService.error('Error', err.error?.message || 'Invalid code');
        this.loading = false;
      }
    });
  }

  copyKey(): void {
    if (this.setupData?.manualEntryKey && navigator.clipboard) {
      navigator.clipboard.writeText(this.setupData.manualEntryKey).then(() => {
        this.keyCopied = true;
        setTimeout(() => this.keyCopied = false, 3000);
      });
    }
  }

  copyRecoveryCodes(): void {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(this.recoveryCodes.join('\n')).then(() => {
        this.codesCopied = true;
        setTimeout(() => this.codesCopied = false, 3000);
      });
    }
  }

  downloadCodes(): void {
    const blob = new Blob([this.recoveryCodes.join('\n')], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'recovery-codes.txt';
    a.click();
    URL.revokeObjectURL(url);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
