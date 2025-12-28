import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SocialProofService } from '../../../services/social-proof.service';
import { RecentPurchase } from '../../../models/social-proof.models';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-purchase-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './purchase-notification.component.html',
  styleUrls: ['./purchase-notification.component.scss']
})
export class PurchaseNotificationComponent implements OnInit, OnDestroy {
  currentNotification: RecentPurchase | null = null;
  showNotification = false;
  private notifications: RecentPurchase[] = [];
  private currentIndex = 0;
  private subscription?: Subscription;
  private notificationIntervalId: any = null;
  private initialTimeoutId: any = null;

  constructor(private socialProofService: SocialProofService) {}

  ngOnInit(): void {
    // Load initial notifications
    this.loadNotifications();

    // Refresh notifications every 5 minutes
    this.subscription = interval(5 * 60 * 1000)
      .pipe(switchMap(() => this.socialProofService.getRecentPurchases(20)))
      .subscribe(purchases => {
        this.notifications = purchases;
      });

    // Show a new notification every 15 seconds
    this.notificationIntervalId = setInterval(() => {
      this.showNextNotification();
    }, 15000);
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    if (this.notificationIntervalId) {
      clearInterval(this.notificationIntervalId);
    }
    if (this.initialTimeoutId) {
      clearTimeout(this.initialTimeoutId);
    }
  }

  private loadNotifications(): void {
    this.socialProofService.getRecentPurchases(20).subscribe(purchases => {
      this.notifications = purchases;
      // Show first notification after 5 seconds
      this.initialTimeoutId = setTimeout(() => {
        this.showNextNotification();
      }, 5000);
    });
  }

  private showNextNotification(): void {
    if (this.notifications.length === 0) return;

    this.currentNotification = this.notifications[this.currentIndex];
    this.showNotification = true;

    // Hide after 6 seconds
    setTimeout(() => {
      this.showNotification = false;
    }, 6000);

    // Move to next notification
    this.currentIndex = (this.currentIndex + 1) % this.notifications.length;
  }

  closeNotification(): void {
    this.showNotification = false;
  }
}
