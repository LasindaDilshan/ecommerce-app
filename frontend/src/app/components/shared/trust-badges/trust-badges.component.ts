import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trust-badges',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="trust-badges">
      <h3 class="trust-title">Shop with Confidence</h3>
      <div class="badges-grid">
        <div class="badge">
          <div class="badge-icon">🔒</div>
          <div class="badge-text">
            <strong>Secure Payment</strong>
            <span>256-bit SSL Encryption</span>
          </div>
        </div>
        <div class="badge">
          <div class="badge-icon">↩️</div>
          <div class="badge-text">
            <strong>30-Day Returns</strong>
            <span>Money-back guarantee</span>
          </div>
        </div>
        <div class="badge">
          <div class="badge-icon">🚚</div>
          <div class="badge-text">
            <strong>Free Shipping</strong>
            <span>On orders over $50</span>
          </div>
        </div>
        <div class="badge">
          <div class="badge-icon">✓</div>
          <div class="badge-text">
            <strong>Quality Guarantee</strong>
            <span>100% authentic products</span>
          </div>
        </div>
      </div>
      <div class="payment-methods">
        <p>We accept:</p>
        <div class="payment-icons">
          <span class="payment-badge">💳 Visa</span>
          <span class="payment-badge">💳 Mastercard</span>
          <span class="payment-badge">💳 American Express</span>
          <span class="payment-badge">💳 Discover</span>
          <span class="payment-badge">💰 PayPal</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .trust-badges {
      background: var(--bg-secondary);
      padding: 2rem;
      border-radius: 12px;
      margin: 2rem 0;
      border: 1px solid var(--border-color);
    }

    .trust-title {
      text-align: center;
      margin-bottom: 1.5rem;
      color: var(--text-primary);
      font-size: 1.5rem;
    }

    .badges-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .badge {
      display: flex;
      align-items: center;
      gap: 1rem;
      background: var(--bg-card);
      padding: 1rem;
      border-radius: 8px;
      box-shadow: var(--shadow-sm);
      border: 1px solid var(--border-color);
      transition: transform 0.2s;
    }

    .badge:hover {
      transform: translateY(-2px);
    }

    .badge-icon {
      font-size: 2rem;
      flex-shrink: 0;
    }

    .badge-text {
      display: flex;
      flex-direction: column;
    }

    .badge-text strong {
      font-size: 0.95rem;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .badge-text span {
      font-size: 0.8rem;
      color: var(--text-tertiary);
    }

    .payment-methods {
      text-align: center;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border-color);
    }

    .payment-methods p {
      color: var(--text-tertiary);
      margin-bottom: 0.75rem;
      font-size: 0.9rem;
    }

    .payment-icons {
      display: flex;
      justify-content: center;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .payment-badge {
      background: var(--bg-card);
      padding: 0.5rem 1rem;
      border-radius: 6px;
      font-size: 0.85rem;
      box-shadow: var(--shadow-sm);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
    }

    @media (max-width: 768px) {
      .trust-badges {
        padding: 1.5rem;
      }

      .badges-grid {
        grid-template-columns: 1fr;
        gap: 1rem;
      }

      .payment-icons {
        flex-direction: column;
        align-items: center;
      }
    }
  `]
})
export class TrustBadgesComponent {}
