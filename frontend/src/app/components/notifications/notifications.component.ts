import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>Notifications & Preferences</h1>
        <p class="subtitle">Manage your notification and email preferences</p>
      </div>

      <!-- Email Preferences -->
      <div class="prefs-card">
        <h3>Email Notifications</h3>
        <p class="section-desc">Choose which emails you'd like to receive</p>

        <div class="pref-list">
          <div class="pref-item">
            <div class="pref-info">
              <strong>Order Confirmations</strong>
              <small>Receive confirmation when your order is placed</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.orderConfirmation" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Shipping Updates</strong>
              <small>Get notified when your order ships and is delivered</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.shippingUpdates" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Promotional Emails</strong>
              <small>Sales, deals, and special offers</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.promotions" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Newsletter</strong>
              <small>Weekly newsletter with product updates and tips</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.newsletter" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Review Reminders</strong>
              <small>Reminders to review products you've purchased</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.reviewReminders" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Wishlist Alerts</strong>
              <small>Get notified when wishlist items go on sale or are back in stock</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.wishlistAlerts" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Loyalty Points Updates</strong>
              <small>Notifications about points earned and reward availability</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.loyaltyUpdates" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>

          <div class="pref-item">
            <div class="pref-info">
              <strong>Subscription Reminders</strong>
              <small>Upcoming billing and delivery notifications</small>
            </div>
            <label class="toggle">
              <input type="checkbox" [(ngModel)]="prefs.subscriptionReminders" (change)="savePrefs()" />
              <span class="toggle-slider"></span>
            </label>
          </div>
        </div>
      </div>

      <!-- Recent Notifications -->
      <div class="notifications-card">
        <h3>Recent Activity</h3>
        <div class="notification-list">
          <div *ngFor="let notif of notifications" class="notification-item" [class.unread]="!notif.read">
            <div class="notif-icon" [class]="notif.type">{{ getNotifIcon(notif.type) }}</div>
            <div class="notif-content">
              <strong>{{ notif.title }}</strong>
              <p>{{ notif.message }}</p>
              <span class="notif-time">{{ notif.createdAt | date:'medium' }}</span>
            </div>
            <button *ngIf="!notif.read" (click)="markAsRead(notif)" class="mark-read-btn">Mark read</button>
          </div>
          <div *ngIf="notifications.length === 0" class="empty-state">
            <p>No notifications yet</p>
          </div>
        </div>
      </div>

      <div *ngIf="saveSuccess" class="save-toast">Preferences saved successfully!</div>
    </div>
  `,
  styles: [`
    .container { max-width: 800px; margin: 0 auto; padding: 20px; }
    .page-header { margin-bottom: 30px; }
    .page-header h1 { color: var(--text-primary); font-size: 2rem; margin: 0 0 8px; }
    .subtitle { color: var(--text-secondary); margin: 0; }
    .prefs-card, .notifications-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px; margin-bottom: 24px; }
    .prefs-card h3, .notifications-card h3 { margin: 0 0 4px; color: var(--text-primary); font-size: 1.2rem; }
    .section-desc { color: var(--text-secondary); margin: 0 0 20px; font-size: 0.9rem; }
    .pref-list { display: flex; flex-direction: column; }
    .pref-item { display: flex; align-items: center; justify-content: space-between; padding: 16px 0; border-bottom: 1px solid var(--border-color); }
    .pref-item:last-child { border-bottom: none; }
    .pref-info strong { display: block; color: var(--text-primary); font-size: 0.95rem; margin-bottom: 2px; }
    .pref-info small { color: var(--text-secondary); font-size: 0.85rem; }
    .toggle { position: relative; display: inline-block; width: 48px; height: 26px; cursor: pointer; }
    .toggle input { opacity: 0; width: 0; height: 0; }
    .toggle-slider { position: absolute; inset: 0; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 26px; transition: all 0.3s; }
    .toggle-slider::before { content: ''; position: absolute; height: 20px; width: 20px; left: 2px; bottom: 2px; background: white; border-radius: 50%; transition: all 0.3s; box-shadow: 0 1px 3px rgba(0,0,0,0.2); }
    .toggle input:checked + .toggle-slider { background: var(--primary); border-color: var(--primary); }
    .toggle input:checked + .toggle-slider::before { transform: translateX(22px); }
    .notification-list { display: flex; flex-direction: column; }
    .notification-item { display: flex; gap: 14px; align-items: flex-start; padding: 16px 0; border-bottom: 1px solid var(--border-color); }
    .notification-item:last-child { border-bottom: none; }
    .notification-item.unread { background: rgba(var(--primary-rgb, 59, 130, 246), 0.05); margin: 0 -24px; padding: 16px 24px; }
    .notif-icon { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; background: var(--bg-secondary); flex-shrink: 0; }
    .notif-icon.order { background: #dbeafe; }
    .notif-icon.shipping { background: #d1fae5; }
    .notif-icon.promo { background: #fef3c7; }
    .notif-icon.loyalty { background: #ede9fe; }
    .notif-content { flex: 1; }
    .notif-content strong { display: block; color: var(--text-primary); font-size: 0.95rem; margin-bottom: 2px; }
    .notif-content p { margin: 0 0 4px; color: var(--text-secondary); font-size: 0.9rem; }
    .notif-time { font-size: 0.8rem; color: var(--text-tertiary); }
    .mark-read-btn { padding: 4px 10px; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 6px; cursor: pointer; font-size: 0.8rem; color: var(--text-secondary); white-space: nowrap; }
    .empty-state { text-align: center; padding: 30px; color: var(--text-secondary); }
    .save-toast { position: fixed; bottom: 20px; right: 20px; background: var(--success); color: white; padding: 12px 24px; border-radius: 8px; font-weight: 600; box-shadow: var(--shadow-lg); animation: fadeInUp 0.3s ease; z-index: 1000; }
    @keyframes fadeInUp { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
    @media (max-width: 600px) { .notification-item.unread { margin: 0 -16px; padding: 12px 16px; } }
  `]
})
export class NotificationsComponent implements OnInit, OnDestroy {
  prefs = {
    orderConfirmation: true,
    shippingUpdates: true,
    promotions: true,
    newsletter: true,
    reviewReminders: true,
    wishlistAlerts: true,
    loyaltyUpdates: true,
    subscriptionReminders: true
  };

  notifications: any[] = [
    { id: 1, type: 'order', title: 'Order Confirmed', message: 'Your order #ORD-1234 has been confirmed.', createdAt: new Date(), read: false },
    { id: 2, type: 'shipping', title: 'Order Shipped', message: 'Your order #ORD-1230 has been shipped.', createdAt: new Date(Date.now() - 86400000), read: true },
    { id: 3, type: 'promo', title: 'Flash Sale!', message: 'Up to 50% off on electronics. Limited time offer!', createdAt: new Date(Date.now() - 172800000), read: true },
    { id: 4, type: 'loyalty', title: 'Points Earned', message: 'You earned 150 loyalty points from your recent purchase.', createdAt: new Date(Date.now() - 259200000), read: false }
  ];

  saveSuccess = false;
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadPrefs();
  }

  loadPrefs(): void {
    try {
      const saved = localStorage.getItem('notification-prefs');
      if (saved) {
        this.prefs = { ...this.prefs, ...JSON.parse(saved) };
      }
    } catch {}
  }

  savePrefs(): void {
    try {
      localStorage.setItem('notification-prefs', JSON.stringify(this.prefs));
      this.saveSuccess = true;
      setTimeout(() => this.saveSuccess = false, 2000);
    } catch {}
  }

  markAsRead(notif: any): void {
    notif.read = true;
  }

  getNotifIcon(type: string): string {
    switch (type) {
      case 'order': return '\uD83D\uDCE6';
      case 'shipping': return '\uD83D\uDE9A';
      case 'promo': return '\uD83C\uDF89';
      case 'loyalty': return '\u2B50';
      default: return '\uD83D\uDD14';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
