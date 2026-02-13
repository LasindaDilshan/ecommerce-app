import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExitIntentService } from '../../../services/exit-intent.service';
import { NewsletterService } from '../../../services/newsletter.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-exit-intent-popup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './exit-intent-popup.component.html',
  styleUrls: ['./exit-intent-popup.component.scss']
})
export class ExitIntentPopupComponent implements OnInit, OnDestroy {
  showPopup = false;
  email = '';
  discountCode = 'WELCOME10';
  copiedToClipboard = false;
  submitting = false;
  submitted = false;
  private subscription?: Subscription;

  constructor(
    private exitIntentService: ExitIntentService,
    private newsletterService: NewsletterService
  ) {}

  ngOnInit(): void {
    this.subscription = this.exitIntentService.showPopup$.subscribe(show => {
      this.showPopup = show;
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  closePopup(): void {
    this.exitIntentService.closePopup();
  }

  claimOffer(): void {
    if (this.email && !this.submitting) {
      this.submitting = true;
      this.newsletterService.subscribe(this.email).subscribe({
        next: (response) => {
          if (response.discountCode) {
            this.discountCode = response.discountCode;
          }
          this.submitted = true;
          this.submitting = false;
          this.copyDiscountCode();
        },
        error: () => {
          this.submitting = false;
          this.copyDiscountCode();
        }
      });
    }
  }

  copyDiscountCode(): void {
    if (typeof navigator !== 'undefined' && navigator.clipboard) {
      navigator.clipboard.writeText(this.discountCode).then(() => {
        this.copiedToClipboard = true;
        setTimeout(() => {
          this.copiedToClipboard = false;
        }, 3000);
      });
    }
  }
}
