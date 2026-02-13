import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NewsletterSignupComponent } from '../newsletter-signup/newsletter-signup.component';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink, NewsletterSignupComponent],
  template: `
    <!-- Newsletter Section -->
    <app-newsletter-signup></app-newsletter-signup>

    <footer class="footer">
      <div class="container">
        <div class="footer-grid">
          <div class="footer-section">
            <h4>Shop Ease</h4>
            <p class="footer-desc">Your one-stop destination for quality products at great prices.</p>
          </div>
          <div class="footer-section">
            <h4>Shop</h4>
            <ul>
              <li><a routerLink="/products">All Products</a></li>
              <li><a routerLink="/search">Search</a></li>
              <li><a routerLink="/comparison">Compare</a></li>
            </ul>
          </div>
          <div class="footer-section">
            <h4>Account</h4>
            <ul>
              <li><a routerLink="/profile">My Profile</a></li>
              <li><a routerLink="/orders">My Orders</a></li>
              <li><a routerLink="/wishlist">Wishlist</a></li>
              <li><a routerLink="/loyalty">Loyalty Rewards</a></li>
            </ul>
          </div>
          <div class="footer-section">
            <h4>Support</h4>
            <ul>
              <li><a routerLink="/track-order">Track Order</a></li>
              <li><a routerLink="/subscriptions">Subscriptions</a></li>
              <li><a routerLink="/notifications">Notifications</a></li>
            </ul>
          </div>
        </div>
        <div class="footer-bottom">
          <p>&copy; {{ currentYear }} Shop Ease. All rights reserved.</p>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: var(--bg-secondary);
      color: var(--text-primary);
      padding: 40px 0 20px;
      margin-top: auto;
      border-top: 1px solid var(--border-color);
    }
    .container { max-width: 1200px; margin: 0 auto; padding: 0 20px; }
    .footer-grid { display: grid; grid-template-columns: 2fr 1fr 1fr 1fr; gap: 30px; margin-bottom: 30px; }
    .footer-section h4 { margin: 0 0 12px; font-size: 1.1rem; color: var(--text-primary); }
    .footer-desc { color: var(--text-secondary); font-size: 0.9rem; line-height: 1.6; margin: 0; }
    .footer-section ul { list-style: none; padding: 0; margin: 0; }
    .footer-section li { margin-bottom: 8px; }
    .footer-section a { color: var(--text-secondary); text-decoration: none; font-size: 0.9rem; transition: color 0.2s; }
    .footer-section a:hover { color: var(--primary); }
    .footer-bottom { text-align: center; padding-top: 20px; border-top: 1px solid var(--border-color); }
    .footer-bottom p { margin: 0; color: var(--text-secondary); font-size: 0.85rem; }
    @media (max-width: 768px) {
      .footer-grid { grid-template-columns: 1fr 1fr; gap: 20px; }
    }
    @media (max-width: 480px) {
      .footer-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}
