import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { SubscriptionService } from '../../services/subscription.service';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { SubscriptionPlan, Subscription, SubscriptionStatus } from '../../models/subscription.models';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>Subscriptions</h1>
        <p class="subtitle">Manage your subscription plans and deliveries</p>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button (click)="activeTab = 'plans'" [class.active]="activeTab === 'plans'" class="tab-btn">Available Plans</button>
        <button (click)="activeTab = 'my'" [class.active]="activeTab === 'my'" class="tab-btn">My Subscriptions</button>
      </div>

      <!-- Available Plans -->
      <div *ngIf="activeTab === 'plans'" class="plans-grid">
        <div *ngFor="let plan of plans" class="plan-card" [class.popular]="plan.name.toLowerCase().includes('premium')">
          <div *ngIf="plan.name.toLowerCase().includes('premium')" class="popular-badge">Most Popular</div>
          <h3>{{ plan.name }}</h3>
          <div class="plan-price">
            <span class="price">\${{ plan.price.toFixed(2) }}</span>
            <span class="interval">/ {{ plan.billingInterval.toLowerCase() }}</span>
          </div>
          <p class="plan-desc">{{ plan.description }}</p>
          <ul class="features-list" *ngIf="plan.features && plan.features.length > 0">
            <li *ngFor="let feature of plan.features">{{ feature }}</li>
          </ul>
          <div *ngIf="plan.trialPeriodDays" class="trial-badge">
            {{ plan.trialPeriodDays }}-day free trial
          </div>
          <div *ngIf="plan.setupFee" class="setup-fee">
            Setup fee: \${{ plan.setupFee.toFixed(2) }}
          </div>
          <button (click)="subscribeToPlan(plan)" class="btn btn-primary btn-full">Subscribe</button>
        </div>
        <div *ngIf="plans.length === 0" class="empty-state">
          <p>No subscription plans available at the moment.</p>
        </div>
      </div>

      <!-- My Subscriptions -->
      <div *ngIf="activeTab === 'my'" class="subscriptions-list">
        <div *ngFor="let sub of subscriptions" class="subscription-card">
          <div class="sub-header">
            <div>
              <h3>{{ sub.planName }}</h3>
              <span class="sub-number">#{{ sub.subscriptionNumber }}</span>
            </div>
            <span class="status-badge" [class]="sub.status.toLowerCase()">{{ sub.status }}</span>
          </div>

          <div class="sub-details">
            <div class="detail-row">
              <span class="label">Price:</span>
              <span class="value">\${{ sub.currentPrice.toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Started:</span>
              <span class="value">{{ sub.startDate | date:'mediumDate' }}</span>
            </div>
            <div class="detail-row" *ngIf="sub.nextBillingDate">
              <span class="label">Next Billing:</span>
              <span class="value">{{ sub.nextBillingDate | date:'mediumDate' }}</span>
            </div>
            <div class="detail-row" *ngIf="sub.trialEndDate">
              <span class="label">Trial Ends:</span>
              <span class="value">{{ sub.trialEndDate | date:'mediumDate' }}</span>
            </div>
            <div class="detail-row" *ngIf="sub.pausedUntil">
              <span class="label">Paused Until:</span>
              <span class="value">{{ sub.pausedUntil | date:'mediumDate' }}</span>
            </div>
          </div>

          <!-- Recent Payments -->
          <div class="payments-section" *ngIf="sub.recentPayments && sub.recentPayments.length > 0">
            <h4>Recent Payments</h4>
            <div *ngFor="let payment of sub.recentPayments.slice(0, 3)" class="payment-row">
              <span>\${{ payment.amount.toFixed(2) }}</span>
              <span class="payment-status" [class]="payment.status.toLowerCase()">{{ payment.status }}</span>
              <span class="payment-date">{{ payment.paymentDate | date:'shortDate' }}</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="sub-actions">
            <button *ngIf="sub.status === 'Active'" (click)="pauseSubscription(sub)" class="btn btn-secondary">Pause</button>
            <button *ngIf="sub.status === 'Paused'" (click)="resumeSubscription(sub)" class="btn btn-primary">Resume</button>
            <button *ngIf="sub.status === 'Active' || sub.status === 'Paused'" (click)="cancelSubscription(sub)" class="btn btn-danger">Cancel</button>
            <button *ngIf="sub.status === 'Cancelled'" (click)="reactivateSubscription(sub)" class="btn btn-primary">Reactivate</button>
          </div>
        </div>

        <div *ngIf="subscriptions.length === 0" class="empty-state">
          <p>You don't have any active subscriptions.</p>
          <button (click)="activeTab = 'plans'" class="btn btn-primary">Browse Plans</button>
        </div>
      </div>

      <!-- Pause Modal -->
      <div class="modal-overlay" *ngIf="showPauseModal" (click)="showPauseModal = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Pause Subscription</h3>
          <div class="form-group">
            <label>Pause until:</label>
            <input type="date" [(ngModel)]="pauseUntilDate" class="form-control" />
          </div>
          <div class="form-group">
            <label>Reason (optional):</label>
            <textarea [(ngModel)]="pauseReason" class="form-control" rows="2"></textarea>
          </div>
          <div class="modal-actions">
            <button (click)="confirmPause()" [disabled]="!pauseUntilDate" class="btn btn-primary">Pause Subscription</button>
            <button (click)="showPauseModal = false" class="btn btn-secondary">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
    .page-header { margin-bottom: 30px; }
    .page-header h1 { color: var(--text-primary); font-size: 2rem; margin: 0 0 8px; }
    .subtitle { color: var(--text-secondary); margin: 0; }
    .tabs { display: flex; gap: 4px; margin-bottom: 24px; border-bottom: 2px solid var(--border-color); }
    .tab-btn { padding: 12px 24px; background: none; border: none; border-bottom: 2px solid transparent; margin-bottom: -2px; cursor: pointer; color: var(--text-secondary); font-weight: 600; font-size: 0.95rem; }
    .tab-btn.active { color: var(--primary); border-bottom-color: var(--primary); }
    .plans-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
    .plan-card { background: var(--bg-card); border: 2px solid var(--border-color); border-radius: 16px; padding: 28px; position: relative; transition: transform 0.2s, box-shadow 0.2s; }
    .plan-card:hover { transform: translateY(-4px); box-shadow: var(--shadow-lg); }
    .plan-card.popular { border-color: var(--primary); }
    .popular-badge { position: absolute; top: -12px; left: 50%; transform: translateX(-50%); background: var(--primary); color: white; padding: 4px 16px; border-radius: 12px; font-size: 0.8rem; font-weight: 600; white-space: nowrap; }
    .plan-card h3 { margin: 0 0 12px; color: var(--text-primary); font-size: 1.3rem; }
    .plan-price { margin-bottom: 12px; }
    .price { font-size: 2.5rem; font-weight: 700; color: var(--text-primary); }
    .interval { color: var(--text-secondary); font-size: 1rem; }
    .plan-desc { color: var(--text-secondary); margin: 0 0 16px; font-size: 0.9rem; }
    .features-list { list-style: none; padding: 0; margin: 0 0 16px; }
    .features-list li { padding: 6px 0; color: var(--text-primary); font-size: 0.9rem; }
    .features-list li::before { content: '\u2713 '; color: var(--success); font-weight: 700; margin-right: 8px; }
    .trial-badge { background: #d1fae5; color: #065f46; padding: 6px 12px; border-radius: 8px; font-size: 0.85rem; font-weight: 600; text-align: center; margin-bottom: 12px; }
    .setup-fee { color: var(--text-tertiary); font-size: 0.85rem; margin-bottom: 12px; }
    .subscriptions-list { display: flex; flex-direction: column; gap: 20px; }
    .subscription-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px; }
    .sub-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px; }
    .sub-header h3 { margin: 0 0 4px; color: var(--text-primary); }
    .sub-number { font-size: 0.85rem; color: var(--text-tertiary); }
    .status-badge { padding: 4px 12px; border-radius: 12px; font-size: 0.8rem; font-weight: 600; }
    .status-badge.active { background: #d1fae5; color: #065f46; }
    .status-badge.trial { background: #dbeafe; color: #1e40af; }
    .status-badge.paused { background: #fef3c7; color: #92400e; }
    .status-badge.cancelled { background: #fecaca; color: #991b1b; }
    .status-badge.pastdue { background: #fecaca; color: #991b1b; }
    .sub-details { display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px; margin-bottom: 16px; }
    .detail-row { display: flex; justify-content: space-between; padding: 6px 0; }
    .label { color: var(--text-secondary); font-size: 0.9rem; }
    .value { color: var(--text-primary); font-weight: 500; font-size: 0.9rem; }
    .payments-section { margin-bottom: 16px; border-top: 1px solid var(--border-color); padding-top: 12px; }
    .payments-section h4 { margin: 0 0 8px; color: var(--text-primary); font-size: 0.95rem; }
    .payment-row { display: flex; gap: 12px; align-items: center; padding: 6px 0; font-size: 0.85rem; }
    .payment-status { padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 600; }
    .payment-status.completed { background: #d1fae5; color: #065f46; }
    .payment-status.failed { background: #fecaca; color: #991b1b; }
    .payment-status.pending { background: #fef3c7; color: #92400e; }
    .payment-date { color: var(--text-tertiary); margin-left: auto; }
    .sub-actions { display: flex; gap: 8px; flex-wrap: wrap; }
    .empty-state { text-align: center; padding: 40px; color: var(--text-secondary); }
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .modal { background: var(--bg-card); border-radius: 16px; padding: 28px; max-width: 450px; width: 90%; }
    .modal h3 { margin: 0 0 16px; color: var(--text-primary); }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 600; color: var(--text-primary); font-size: 0.9rem; }
    .form-control { width: 100%; padding: 10px 14px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-secondary); color: var(--text-primary); font-size: 1rem; box-sizing: border-box; font-family: inherit; }
    .modal-actions { display: flex; gap: 8px; }
    .btn { padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.9rem; }
    .btn-full { width: 100%; }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-secondary { background: var(--bg-secondary); color: var(--text-primary); border: 1px solid var(--border-color); }
    .btn-danger { background: var(--danger); color: white; }
    @media (max-width: 768px) {
      .plans-grid { grid-template-columns: 1fr; }
      .sub-details { grid-template-columns: 1fr; }
    }
  `]
})
export class SubscriptionsComponent implements OnInit, OnDestroy {
  plans: SubscriptionPlan[] = [];
  subscriptions: Subscription[] = [];
  activeTab = 'plans';
  showPauseModal = false;
  pauseUntilDate = '';
  pauseReason = '';
  private selectedSub: Subscription | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private subscriptionService: SubscriptionService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadPlans();
    this.loadSubscriptions();
  }

  loadPlans(): void {
    this.subscriptionService.getPlans().pipe(takeUntil(this.destroy$)).subscribe({
      next: (plans) => this.plans = plans,
      error: () => {}
    });
  }

  loadSubscriptions(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.subscriptionService.getUserSubscriptions(user.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (subs) => this.subscriptions = subs,
        error: () => {}
      });
    }
  }

  subscribeToPlan(plan: SubscriptionPlan): void {
    const user = this.authService.getCurrentUser();
    if (!user) return;

    this.subscriptionService.createSubscription({
      userId: user.id,
      planId: plan.planId,
      startTrial: !!plan.trialPeriodDays
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Subscribed!', `You've been subscribed to ${plan.name}`);
        this.activeTab = 'my';
        this.loadSubscriptions();
      },
      error: (err) => this.toastService.error('Error', err.error?.message || 'Failed to subscribe')
    });
  }

  pauseSubscription(sub: Subscription): void {
    this.selectedSub = sub;
    this.showPauseModal = true;
  }

  confirmPause(): void {
    if (!this.selectedSub || !this.pauseUntilDate) return;

    this.subscriptionService.pauseSubscription(this.selectedSub.subscriptionId, {
      pauseUntil: new Date(this.pauseUntilDate),
      reason: this.pauseReason
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Paused', 'Subscription has been paused');
        this.showPauseModal = false;
        this.pauseUntilDate = '';
        this.pauseReason = '';
        this.loadSubscriptions();
      },
      error: (err) => this.toastService.error('Error', err.error?.message || 'Failed to pause')
    });
  }

  resumeSubscription(sub: Subscription): void {
    this.subscriptionService.resumeSubscription(sub.subscriptionId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toastService.success('Resumed', 'Subscription has been resumed'); this.loadSubscriptions(); },
      error: (err) => this.toastService.error('Error', err.error?.message || 'Failed to resume')
    });
  }

  cancelSubscription(sub: Subscription): void {
    if (!confirm('Are you sure you want to cancel this subscription?')) return;

    this.subscriptionService.cancelSubscription(sub.subscriptionId, {
      cancelImmediately: false,
      reason: 'User requested cancellation'
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toastService.success('Cancelled', 'Subscription has been cancelled'); this.loadSubscriptions(); },
      error: (err) => this.toastService.error('Error', err.error?.message || 'Failed to cancel')
    });
  }

  reactivateSubscription(sub: Subscription): void {
    this.subscriptionService.reactivateSubscription(sub.subscriptionId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toastService.success('Reactivated', 'Subscription has been reactivated'); this.loadSubscriptions(); },
      error: (err) => this.toastService.error('Error', err.error?.message || 'Failed to reactivate')
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
