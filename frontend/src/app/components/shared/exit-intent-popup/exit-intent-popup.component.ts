import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExitIntentService } from '../../../services/exit-intent.service';
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
  private subscription?: Subscription;

  constructor(private exitIntentService: ExitIntentService) {}

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
    if (this.email) {
      // In a real app, you would send this to your backend
      console.log('Email submitted:', this.email);
      this.copyDiscountCode();
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
