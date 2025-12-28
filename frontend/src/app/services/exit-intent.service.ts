import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, fromEvent, Subscription } from 'rxjs';
import { filter, throttleTime } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ExitIntentService implements OnDestroy {
  private readonly STORAGE_KEY = 'exitIntentShown';
  private readonly COOLDOWN_DAYS = 7; // Show popup again after 7 days
  private showPopupSubject = new BehaviorSubject<boolean>(false);
  private exitIntentSubscription?: Subscription;

  showPopup$: Observable<boolean> = this.showPopupSubject.asObservable();

  constructor() {
    this.initExitIntentDetection();
  }

  ngOnDestroy(): void {
    this.exitIntentSubscription?.unsubscribe();
  }

  private initExitIntentDetection(): void {
    // Only run in browser environment
    if (typeof window === 'undefined') return;

    // Listen for mouse leaving the viewport
    this.exitIntentSubscription = fromEvent<MouseEvent>(document, 'mouseleave')
      .pipe(
        throttleTime(1000), // Throttle to prevent multiple triggers
        filter((event) => {
          // Check if mouse is leaving from the top of the page
          return event.clientY <= 0 && !this.hasShownRecently();
        })
      )
      .subscribe(() => {
        this.triggerExitIntent();
      });
  }

  private triggerExitIntent(): void {
    if (!this.hasShownRecently()) {
      this.showPopupSubject.next(true);
      this.markAsShown();
    }
  }

  closePopup(): void {
    this.showPopupSubject.next(false);
  }

  private hasShownRecently(): boolean {
    try {
      const lastShown = localStorage.getItem(this.STORAGE_KEY);
      if (!lastShown) return false;

      const lastShownDate = new Date(lastShown);
      const daysSinceShown = (Date.now() - lastShownDate.getTime()) / (1000 * 60 * 60 * 24);
      return daysSinceShown < this.COOLDOWN_DAYS;
    } catch (error) {
      console.error('Error checking exit intent status:', error);
      return false;
    }
  }

  private markAsShown(): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, new Date().toISOString());
    } catch (error) {
      console.error('Error saving exit intent status:', error);
    }
  }
}
