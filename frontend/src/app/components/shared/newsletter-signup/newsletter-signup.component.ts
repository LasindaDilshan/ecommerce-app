import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsletterService } from '../../../services/newsletter.service';

@Component({
  selector: 'app-newsletter-signup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="newsletter-section">
      <div class="newsletter-content">
        <div class="newsletter-text">
          <h3>Stay in the Loop</h3>
          <p>Subscribe to our newsletter and get <strong>5% off</strong> your next order!</p>
        </div>
        <div class="newsletter-form" *ngIf="!subscribed">
          <div class="input-group">
            <input
              type="email"
              [(ngModel)]="email"
              placeholder="Enter your email address"
              class="email-input"
              [disabled]="submitting"
              (keyup.enter)="subscribe()"
            />
            <button
              (click)="subscribe()"
              [disabled]="!email || submitting"
              class="subscribe-btn">
              {{ submitting ? 'Subscribing...' : 'Subscribe' }}
            </button>
          </div>
          <p *ngIf="errorMessage" class="error-msg">{{ errorMessage }}</p>
        </div>
        <div class="newsletter-success" *ngIf="subscribed">
          <p class="success-msg">Thanks for subscribing!</p>
          <p *ngIf="discountCode" class="discount-code">
            Your discount code: <strong>{{ discountCode }}</strong>
            <button (click)="copyCode()" class="copy-btn">{{ copied ? 'Copied!' : 'Copy' }}</button>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .newsletter-section {
      background: linear-gradient(135deg, var(--primary), #6366f1);
      padding: 40px 20px;
      text-align: center;
    }
    .newsletter-content { max-width: 600px; margin: 0 auto; }
    .newsletter-text h3 { color: white; font-size: 1.5rem; margin: 0 0 8px 0; }
    .newsletter-text p { color: rgba(255,255,255,0.9); margin: 0 0 20px 0; }
    .newsletter-text strong { color: #fbbf24; }
    .input-group { display: flex; gap: 8px; max-width: 500px; margin: 0 auto; }
    .email-input {
      flex: 1; padding: 12px 16px; border: 2px solid rgba(255,255,255,0.3);
      border-radius: 8px; font-size: 1rem; background: rgba(255,255,255,0.15);
      color: white; outline: none; transition: border-color 0.2s;
    }
    .email-input::placeholder { color: rgba(255,255,255,0.6); }
    .email-input:focus { border-color: white; }
    .subscribe-btn {
      padding: 12px 24px; background: white; color: var(--primary); border: none;
      border-radius: 8px; font-weight: 600; cursor: pointer; font-size: 1rem;
      transition: transform 0.2s, box-shadow 0.2s; white-space: nowrap;
    }
    .subscribe-btn:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.2); }
    .subscribe-btn:disabled { opacity: 0.7; cursor: not-allowed; }
    .error-msg { color: #fca5a5; margin: 8px 0 0; font-size: 0.9rem; }
    .success-msg { color: #86efac; font-size: 1.1rem; font-weight: 600; margin: 0 0 8px; }
    .discount-code { color: white; font-size: 1rem; margin: 0; display: flex; align-items: center; justify-content: center; gap: 10px; }
    .discount-code strong { background: rgba(255,255,255,0.2); padding: 6px 12px; border-radius: 6px; letter-spacing: 1px; }
    .copy-btn {
      padding: 4px 12px; background: rgba(255,255,255,0.2); color: white; border: 1px solid rgba(255,255,255,0.4);
      border-radius: 4px; cursor: pointer; font-size: 0.85rem; transition: background 0.2s;
    }
    .copy-btn:hover { background: rgba(255,255,255,0.3); }
    @media (max-width: 600px) {
      .input-group { flex-direction: column; }
      .subscribe-btn { width: 100%; }
    }
  `]
})
export class NewsletterSignupComponent {
  email = '';
  submitting = false;
  subscribed = false;
  discountCode = '';
  errorMessage = '';
  copied = false;

  constructor(private newsletterService: NewsletterService) {}

  subscribe(): void {
    if (!this.email || this.submitting) return;

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) {
      this.errorMessage = 'Please enter a valid email address';
      return;
    }

    this.submitting = true;
    this.errorMessage = '';

    this.newsletterService.subscribe(this.email).subscribe({
      next: (response) => {
        this.subscribed = true;
        this.discountCode = response.discountCode || '';
        this.submitting = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to subscribe. Please try again.';
        this.submitting = false;
      }
    });
  }

  copyCode(): void {
    if (this.discountCode && navigator.clipboard) {
      navigator.clipboard.writeText(this.discountCode).then(() => {
        this.copied = true;
        setTimeout(() => this.copied = false, 3000);
      });
    }
  }
}
