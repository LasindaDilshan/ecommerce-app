import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-offline',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="offline-container">
      <div class="offline-content">
        <div class="offline-icon">📡</div>
        <h1>You're Offline</h1>
        <p>It looks like you've lost your internet connection.</p>
        <p class="secondary-text">
          Don't worry! You can still browse products you've already viewed.
        </p>
        <div class="offline-actions">
          <button (click)="retry()" class="retry-btn">
            <span class="retry-icon">🔄</span>
            Try Again
          </button>
          <a routerLink="/products" class="browse-btn">
            Browse Cached Products
          </a>
        </div>
        <div class="tips">
          <h3>While you're offline, you can:</h3>
          <ul>
            <li>✓ View previously loaded products</li>
            <li>✓ Browse cached categories</li>
            <li>✓ Add items to your wishlist (syncs when online)</li>
            <li>✓ Prepare your shopping cart</li>
          </ul>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .offline-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 2rem;
    }

    .offline-content {
      background: white;
      border-radius: 16px;
      padding: 3rem;
      max-width: 600px;
      text-align: center;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      animation: slideUp 0.5s ease-out;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .offline-icon {
      font-size: 5rem;
      margin-bottom: 1rem;
      animation: pulse 2s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
      }
      50% {
        transform: scale(1.1);
      }
    }

    h1 {
      font-size: 2.5rem;
      color: #333;
      margin-bottom: 1rem;
    }

    p {
      font-size: 1.1rem;
      color: #666;
      margin-bottom: 0.5rem;
    }

    .secondary-text {
      font-size: 1rem;
      color: #888;
      margin-bottom: 2rem;
    }

    .offline-actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      margin-bottom: 3rem;
      flex-wrap: wrap;
    }

    .retry-btn, .browse-btn {
      padding: 1rem 2rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .retry-btn {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
    }

    .retry-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 16px rgba(102, 126, 234, 0.4);
    }

    .retry-icon {
      display: inline-block;
      transition: transform 0.3s;
    }

    .retry-btn:hover .retry-icon {
      transform: rotate(180deg);
    }

    .browse-btn {
      background: white;
      color: #667eea;
      border: 2px solid #667eea;
    }

    .browse-btn:hover {
      background: #667eea;
      color: white;
      transform: translateY(-2px);
      box-shadow: 0 8px 16px rgba(102, 126, 234, 0.4);
    }

    .tips {
      background: #f8f9fa;
      border-radius: 12px;
      padding: 2rem;
      text-align: left;
    }

    .tips h3 {
      color: #333;
      margin-bottom: 1rem;
      font-size: 1.2rem;
    }

    .tips ul {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .tips li {
      padding: 0.5rem 0;
      color: #666;
      font-size: 1rem;
    }

    @media (max-width: 768px) {
      .offline-content {
        padding: 2rem;
      }

      h1 {
        font-size: 2rem;
      }

      .offline-icon {
        font-size: 4rem;
      }

      .offline-actions {
        flex-direction: column;
      }

      .retry-btn, .browse-btn {
        width: 100%;
      }
    }
  `]
})
export class OfflineComponent {
  retry(): void {
    window.location.reload();
  }
}
