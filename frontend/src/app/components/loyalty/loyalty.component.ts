import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { LoyaltyService } from '../../services/loyalty.service';
import { LoyaltySummary, LoyaltyReward, LoyaltyTransaction, RedeemedReward } from '../../models/loyalty.models';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-loyalty',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>Loyalty Rewards</h1>
        <p class="subtitle">Earn points on every purchase and redeem for exclusive rewards</p>
      </div>

      <!-- Points Overview -->
      <div class="points-overview" *ngIf="summary">
        <div class="points-card main">
          <div class="points-value">{{ summary.currentPoints }}</div>
          <div class="points-label">Available Points</div>
        </div>
        <div class="points-card">
          <div class="points-value">{{ summary.lifetimePoints }}</div>
          <div class="points-label">Lifetime Points</div>
        </div>
        <div class="points-card tier" [class]="summary.tier.toLowerCase()">
          <div class="tier-icon">
            {{ getTierIcon(summary.tier) }}
          </div>
          <div class="points-value">{{ summary.tier }}</div>
          <div class="points-label">Current Tier</div>
        </div>
        <div class="points-card">
          <div class="points-value">{{ summary.earningMultiplier }}x</div>
          <div class="points-label">Points Multiplier</div>
        </div>
      </div>

      <!-- Tier Progress -->
      <div class="tier-progress" *ngIf="summary && summary.pointsToNextTier > 0">
        <h3>Progress to {{ summary.nextTier }}</h3>
        <div class="progress-bar">
          <div class="progress-fill" [style.width.%]="getTierProgress()"></div>
        </div>
        <p class="progress-text">{{ summary.pointsToNextTier }} more points needed</p>
      </div>

      <!-- Tier Benefits -->
      <div class="benefits-section" *ngIf="summary && summary.tierBenefits.length > 0">
        <h3>Your Tier Benefits</h3>
        <div class="benefits-grid">
          <div *ngFor="let benefit of summary.tierBenefits" class="benefit-item">{{ benefit }}</div>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button (click)="activeTab = 'rewards'" [class.active]="activeTab === 'rewards'" class="tab-btn">Available Rewards</button>
        <button (click)="activeTab = 'redeemed'" [class.active]="activeTab === 'redeemed'" class="tab-btn">My Rewards</button>
        <button (click)="activeTab = 'history'" [class.active]="activeTab === 'history'" class="tab-btn">Points History</button>
      </div>

      <!-- Available Rewards -->
      <div class="tab-content" *ngIf="activeTab === 'rewards'">
        <div class="rewards-grid">
          <div *ngFor="let reward of rewards" class="reward-card" [class.disabled]="!reward.canRedeem">
            <div class="reward-header">
              <span class="reward-type">{{ reward.type }}</span>
              <span class="reward-cost">{{ reward.pointsCost }} pts</span>
            </div>
            <h4>{{ reward.name }}</h4>
            <p>{{ reward.description }}</p>
            <div *ngIf="reward.minimumTier" class="min-tier">Requires {{ reward.minimumTier }} tier</div>
            <button
              (click)="redeemReward(reward)"
              [disabled]="!reward.canRedeem || redeeming"
              class="btn btn-primary">
              {{ reward.canRedeem ? 'Redeem' : 'Not Enough Points' }}
            </button>
          </div>
          <div *ngIf="rewards.length === 0" class="empty-state">
            <p>No rewards available at the moment. Check back soon!</p>
          </div>
        </div>
      </div>

      <!-- Redeemed Rewards -->
      <div class="tab-content" *ngIf="activeTab === 'redeemed'">
        <div class="redeemed-list">
          <div *ngFor="let item of redeemedRewards" class="redeemed-card">
            <div class="redeemed-info">
              <h4>{{ item.rewardName }}</h4>
              <p class="code">Code: <strong>{{ item.redemptionCode }}</strong></p>
              <p class="meta">
                <span>{{ item.pointsSpent }} pts spent</span>
                <span [class.expired]="isExpired(item)">{{ isExpired(item) ? 'Expired' : (item.isUsed ? 'Used' : 'Active') }}</span>
              </p>
            </div>
            <div class="redeemed-dates">
              <p>Redeemed: {{ item.redeemedAt | date:'shortDate' }}</p>
              <p>Expires: {{ item.expiresAt | date:'shortDate' }}</p>
            </div>
          </div>
          <div *ngIf="redeemedRewards.length === 0" class="empty-state">
            <p>You haven't redeemed any rewards yet.</p>
          </div>
        </div>
      </div>

      <!-- Transaction History -->
      <div class="tab-content" *ngIf="activeTab === 'history'">
        <div class="transaction-list">
          <div *ngFor="let tx of transactions" class="transaction-card">
            <div class="tx-icon" [class.earn]="tx.points > 0" [class.spend]="tx.points < 0">
              {{ tx.points > 0 ? '+' : '' }}{{ tx.points }}
            </div>
            <div class="tx-info">
              <strong>{{ tx.description }}</strong>
              <span class="tx-date">{{ tx.createdAt | date:'medium' }}</span>
            </div>
            <div class="tx-type">{{ tx.type }}</div>
          </div>
          <div *ngIf="transactions.length === 0" class="empty-state">
            <p>No transaction history yet. Start shopping to earn points!</p>
          </div>
        </div>
      </div>

      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading loyalty data...</p>
      </div>
    </div>
  `,
  styles: [`
    .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
    .page-header { margin-bottom: 30px; }
    .page-header h1 { color: var(--text-primary); font-size: 2rem; margin: 0 0 8px; }
    .subtitle { color: var(--text-secondary); margin: 0; }
    .points-overview { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 30px; }
    .points-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px; text-align: center; box-shadow: var(--shadow-sm); }
    .points-card.main { background: linear-gradient(135deg, var(--primary), #6366f1); color: white; border: none; }
    .points-card.main .points-label { color: rgba(255,255,255,0.8); }
    .points-card.tier.bronze { border-color: #cd7f32; }
    .points-card.tier.silver { border-color: #c0c0c0; }
    .points-card.tier.gold { border-color: #ffd700; }
    .points-card.tier.platinum { border-color: #e5e4e2; }
    .tier-icon { font-size: 2rem; margin-bottom: 4px; }
    .points-value { font-size: 2rem; font-weight: 700; color: var(--text-primary); }
    .points-card.main .points-value { color: white; }
    .points-label { font-size: 0.85rem; color: var(--text-secondary); margin-top: 4px; }
    .tier-progress { background: var(--bg-card); padding: 24px; border-radius: 12px; border: 1px solid var(--border-color); margin-bottom: 24px; }
    .tier-progress h3 { margin: 0 0 12px; color: var(--text-primary); font-size: 1.1rem; }
    .progress-bar { height: 12px; background: var(--bg-secondary); border-radius: 6px; overflow: hidden; }
    .progress-fill { height: 100%; background: linear-gradient(90deg, var(--primary), #6366f1); border-radius: 6px; transition: width 0.5s ease; }
    .progress-text { margin: 8px 0 0; font-size: 0.85rem; color: var(--text-secondary); }
    .benefits-section { margin-bottom: 24px; }
    .benefits-section h3 { margin: 0 0 12px; color: var(--text-primary); }
    .benefits-grid { display: flex; flex-wrap: wrap; gap: 8px; }
    .benefit-item { background: var(--bg-card); border: 1px solid var(--border-color); padding: 8px 16px; border-radius: 20px; font-size: 0.9rem; color: var(--text-primary); }
    .tabs { display: flex; gap: 4px; margin-bottom: 24px; border-bottom: 2px solid var(--border-color); }
    .tab-btn { padding: 12px 24px; background: none; border: none; border-bottom: 2px solid transparent; margin-bottom: -2px; cursor: pointer; color: var(--text-secondary); font-weight: 600; font-size: 0.95rem; transition: all 0.2s; }
    .tab-btn.active { color: var(--primary); border-bottom-color: var(--primary); }
    .tab-btn:hover { color: var(--text-primary); }
    .rewards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
    .reward-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; }
    .reward-card.disabled { opacity: 0.6; }
    .reward-header { display: flex; justify-content: space-between; margin-bottom: 8px; }
    .reward-type { background: var(--bg-secondary); padding: 2px 10px; border-radius: 12px; font-size: 0.75rem; font-weight: 600; color: var(--text-secondary); }
    .reward-cost { font-weight: 700; color: var(--primary); }
    .reward-card h4 { margin: 0 0 8px; color: var(--text-primary); }
    .reward-card p { color: var(--text-secondary); font-size: 0.9rem; margin: 0 0 12px; }
    .min-tier { font-size: 0.8rem; color: var(--text-tertiary); margin-bottom: 12px; }
    .redeemed-list, .transaction-list { display: flex; flex-direction: column; gap: 12px; }
    .redeemed-card { display: flex; justify-content: space-between; align-items: center; background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 10px; padding: 16px 20px; }
    .redeemed-info h4 { margin: 0 0 4px; color: var(--text-primary); }
    .redeemed-info .code { margin: 0 0 4px; font-size: 0.9rem; color: var(--text-secondary); }
    .redeemed-info .code strong { color: var(--primary); }
    .redeemed-info .meta { display: flex; gap: 12px; font-size: 0.8rem; color: var(--text-tertiary); margin: 0; }
    .redeemed-info .meta .expired { color: var(--danger); }
    .redeemed-dates p { margin: 0 0 2px; font-size: 0.85rem; color: var(--text-secondary); text-align: right; }
    .transaction-card { display: flex; align-items: center; gap: 16px; background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 10px; padding: 14px 20px; }
    .tx-icon { width: 60px; font-weight: 700; font-size: 1.1rem; text-align: center; }
    .tx-icon.earn { color: var(--success); }
    .tx-icon.spend { color: var(--danger); }
    .tx-info { flex: 1; }
    .tx-info strong { display: block; color: var(--text-primary); font-size: 0.95rem; }
    .tx-date { font-size: 0.8rem; color: var(--text-tertiary); }
    .tx-type { font-size: 0.8rem; color: var(--text-secondary); background: var(--bg-secondary); padding: 4px 10px; border-radius: 12px; }
    .empty-state { text-align: center; padding: 40px; color: var(--text-secondary); }
    .btn { padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; width: 100%; }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .loading { display: flex; flex-direction: column; align-items: center; padding: 40px; color: var(--text-secondary); }
    .spinner { border: 4px solid var(--border-color); border-top: 4px solid var(--primary); border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite; margin-bottom: 12px; }
    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    @media (max-width: 768px) {
      .points-overview { grid-template-columns: repeat(2, 1fr); }
      .redeemed-card { flex-direction: column; align-items: flex-start; gap: 8px; }
      .redeemed-dates { text-align: left; }
    }
  `]
})
export class LoyaltyComponent implements OnInit, OnDestroy {
  summary: LoyaltySummary | null = null;
  rewards: LoyaltyReward[] = [];
  redeemedRewards: RedeemedReward[] = [];
  transactions: LoyaltyTransaction[] = [];
  activeTab = 'rewards';
  loading = true;
  redeeming = false;
  private destroy$ = new Subject<void>();

  constructor(
    private loyaltyService: LoyaltyService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;

    this.loyaltyService.getSummary().pipe(takeUntil(this.destroy$)).subscribe({
      next: (summary) => { this.summary = summary; this.loading = false; },
      error: () => this.loading = false
    });

    this.loyaltyService.getAvailableRewards().pipe(takeUntil(this.destroy$)).subscribe({
      next: (rewards) => this.rewards = rewards,
      error: () => {}
    });

    this.loyaltyService.getRedeemedRewards().pipe(takeUntil(this.destroy$)).subscribe({
      next: (redeemed) => this.redeemedRewards = redeemed,
      error: () => {}
    });

    this.loyaltyService.getTransactions().pipe(takeUntil(this.destroy$)).subscribe({
      next: (transactions) => this.transactions = transactions,
      error: () => {}
    });
  }

  redeemReward(reward: LoyaltyReward): void {
    this.redeeming = true;
    this.loyaltyService.redeemReward(reward.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.toastService.success('Reward Redeemed!', `Your code: ${response.redemptionCode}`);
        this.redeeming = false;
        this.loadData();
      },
      error: (err) => {
        this.toastService.error('Error', err.error?.message || 'Failed to redeem reward');
        this.redeeming = false;
      }
    });
  }

  getTierIcon(tier: string): string {
    switch (tier?.toLowerCase()) {
      case 'bronze': return '\uD83E\uDD49';
      case 'silver': return '\uD83E\uDD48';
      case 'gold': return '\uD83E\uDD47';
      case 'platinum': return '\uD83D\uDC8E';
      default: return '\u2B50';
    }
  }

  getTierProgress(): number {
    if (!this.summary || this.summary.pointsToNextTier <= 0) return 100;
    const totalNeeded = this.summary.currentPoints + this.summary.pointsToNextTier;
    return (this.summary.currentPoints / totalNeeded) * 100;
  }

  isExpired(item: RedeemedReward): boolean {
    return new Date(item.expiresAt) < new Date();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
