import { Injectable } from '@angular/core';
import { SwUpdate, VersionReadyEvent, SwPush } from '@angular/service-worker';
import { filter, map } from 'rxjs/operators';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

interface OfflineAction {
  id: string;
  type: 'cart' | 'wishlist' | 'order';
  action: 'add' | 'remove' | 'update';
  payload: any;
  timestamp: number;
}

@Injectable({
  providedIn: 'root'
})
export class PwaService {
  private promptEvent: any;
  private canInstallSubject = new BehaviorSubject<boolean>(false);
  private updateAvailableSubject = new BehaviorSubject<boolean>(false);
  private updateCheckIntervalId: any = null;
  private offlineActionsKey = 'pwa_offline_actions';
  private pushNotificationSubject = new Subject<any>();

  canInstall$: Observable<boolean> = this.canInstallSubject.asObservable();
  updateAvailable$: Observable<boolean> = this.updateAvailableSubject.asObservable();
  pushNotification$ = this.pushNotificationSubject.asObservable();

  // VAPID public key for push notifications (should be configured in environment)
  private readonly VAPID_PUBLIC_KEY = environment.vapidPublicKey || '';

  constructor(
    private swUpdate: SwUpdate,
    private swPush: SwPush,
    private http: HttpClient
  ) {
    this.initPwaPrompt();
    this.checkForUpdates();
    this.initPushNotifications();
    this.syncOfflineActions();
  }

  private initPwaPrompt(): void {
    window.addEventListener('beforeinstallprompt', (event: any) => {
      event.preventDefault();
      this.promptEvent = event;
      this.canInstallSubject.next(true);
    });

    window.addEventListener('appinstalled', () => {
      this.canInstallSubject.next(false);
      this.promptEvent = null;
    });
  }

  async installPwa(): Promise<boolean> {
    if (!this.promptEvent) {
      return false;
    }

    try {
      this.promptEvent.prompt();
      const result = await this.promptEvent.userChoice;

      if (result.outcome === 'accepted') {
        this.canInstallSubject.next(false);
        return true;
      } else {
        return false;
      }
    } catch (error) {
      return false;
    }
  }

  private checkForUpdates(): void {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    // Check for updates every 6 hours - store interval ID for cleanup
    this.updateCheckIntervalId = setInterval(() => {
      this.swUpdate.checkForUpdate().catch(() => {
        // Silently handle update check failures
      });
    }, 6 * 60 * 60 * 1000);

    // Listen for new versions
    this.swUpdate.versionUpdates
      .pipe(
        filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY'),
        map(evt => ({
          type: 'UPDATE_AVAILABLE',
          current: evt.currentVersion,
          available: evt.latestVersion,
        }))
      )
      .subscribe(() => {
        // Signal that update is available - let component handle UI
        this.updateAvailableSubject.next(true);
      });
  }

  // Method for components to call when user confirms update
  activateUpdate(): void {
    this.swUpdate.activateUpdate()
      .then(() => {
        document.location.reload();
      })
      .catch(() => {
        // Silently handle activation failures
      });
  }

  isOnline(): boolean {
    return navigator.onLine;
  }

  watchOnlineStatus(): Observable<boolean> {
    return new Observable(observer => {
      observer.next(navigator.onLine);

      const onlineHandler = () => {
        observer.next(true);
        this.syncOfflineActions();
      };
      const offlineHandler = () => observer.next(false);

      window.addEventListener('online', onlineHandler);
      window.addEventListener('offline', offlineHandler);

      return () => {
        window.removeEventListener('online', onlineHandler);
        window.removeEventListener('offline', offlineHandler);
      };
    });
  }

  // Push Notification Methods
  private initPushNotifications(): void {
    if (!this.swPush.isEnabled) {
      return;
    }

    // Listen for push messages
    this.swPush.messages.subscribe(message => {
      this.pushNotificationSubject.next(message);
    });

    // Handle notification clicks
    this.swPush.notificationClicks.subscribe(({ action, notification }) => {
      if (notification.data?.url) {
        window.open(notification.data.url, '_blank');
      }
    });
  }

  async subscribeToPushNotifications(): Promise<PushSubscription | null> {
    if (!this.swPush.isEnabled || !this.VAPID_PUBLIC_KEY) {
      return null;
    }

    try {
      const subscription = await this.swPush.requestSubscription({
        serverPublicKey: this.VAPID_PUBLIC_KEY
      });

      // Send subscription to backend
      await this.http.post(`${environment.apiUrl}/api/notifications/subscribe`, subscription).toPromise();

      return subscription;
    } catch (error) {
      return null;
    }
  }

  async unsubscribeFromPushNotifications(): Promise<boolean> {
    if (!this.swPush.isEnabled) {
      return false;
    }

    try {
      await this.swPush.unsubscribe();
      return true;
    } catch (error) {
      return false;
    }
  }

  isPushNotificationEnabled(): boolean {
    return this.swPush.isEnabled;
  }

  private getOfflineActions(): OfflineAction[] {
    const stored = localStorage.getItem(this.offlineActionsKey);
    return stored ? JSON.parse(stored) : [];
  }

  private async syncOfflineActions(): Promise<void> {
    if (!navigator.onLine) {
      return;
    }

    const actions = this.getOfflineActions();
    if (actions.length === 0) {
      return;
    }

    const successfulIds: string[] = [];

    for (const action of actions) {
      try {
        await this.executeOfflineAction(action);
        successfulIds.push(action.id);
      } catch (error) {
        // Continue with next action on failure
      }
    }

    // Remove successfully synced actions
    const remainingActions = actions.filter(a => !successfulIds.includes(a.id));
    localStorage.setItem(this.offlineActionsKey, JSON.stringify(remainingActions));
  }

  private async executeOfflineAction(action: OfflineAction): Promise<void> {
    const apiUrl = environment.apiUrl;

    switch (action.type) {
      case 'cart':
        if (action.action === 'add') {
          await this.http.post(`${apiUrl}/api/cart/items`, action.payload).toPromise();
        } else if (action.action === 'remove') {
          await this.http.delete(`${apiUrl}/api/cart/items/${action.payload.itemId}`).toPromise();
        } else if (action.action === 'update') {
          await this.http.put(`${apiUrl}/api/cart/items/${action.payload.itemId}`, action.payload).toPromise();
        }
        break;

      case 'wishlist':
        if (action.action === 'add') {
          await this.http.post(`${apiUrl}/api/wishlist/items`, action.payload).toPromise();
        } else if (action.action === 'remove') {
          await this.http.delete(`${apiUrl}/api/wishlist/items/${action.payload.productId}`).toPromise();
        }
        break;

      default:
        // Unknown action type - skip silently
        break;
    }
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
