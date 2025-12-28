import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';
import { CartService } from '../../../services/cart.service';
import { WishlistService } from '../../../services/wishlist.service';
import { ThemeService } from '../../../services/theme.service';
import { User } from '../../../models/auth.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="header animate-slideInDown">
      <div class="container">
        <div class="header-content">
          <div class="logo animate-fadeIn">
            <a routerLink="/home">
              <span class="logo-icon">🛍️</span>
              <span class="logo-text">Shop Ease</span>
            </a>
          </div>

          <nav class="nav">
            <a routerLink="/home" routerLinkActive="active" class="nav-link">
              <span class="nav-icon">🏠</span>
              Home
            </a>
            <a routerLink="/products" routerLinkActive="active" class="nav-link">
              <span class="nav-icon">📦</span>
              Products
            </a>

            <ng-container *ngIf="currentUser">
              <a routerLink="/orders" routerLinkActive="active" class="nav-link">
                <span class="nav-icon">📋</span>
                Orders
              </a>
              <a routerLink="/profile" routerLinkActive="active" class="nav-link">
                <span class="nav-icon">👤</span>
                Profile
              </a>

              <ng-container *ngIf="isAdmin">
                <a routerLink="/admin" routerLinkActive="active" class="nav-link admin-link">
                  <span class="nav-icon">⚙️</span>
                  Admin
                </a>
              </ng-container>

              <a routerLink="/wishlist" routerLinkActive="active" class="nav-link icon-link">
                <span class="nav-icon">💝</span>
                <span class="badge wishlist-badge animate-pulse" *ngIf="wishlistItemCount > 0">
                  {{ wishlistItemCount }}
                </span>
              </a>

              <a routerLink="/cart" routerLinkActive="active" class="nav-link icon-link">
                <span class="nav-icon">🛒</span>
                <span class="badge cart-badge animate-pulse" *ngIf="cartItemCount > 0">
                  {{ cartItemCount }}
                </span>
              </a>

              <button (click)="logout()" class="btn btn-ghost">
                <span class="nav-icon">🚪</span>
                Logout
              </button>
            </ng-container>

            <ng-container *ngIf="!currentUser">
              <a routerLink="/login" routerLinkActive="active" class="nav-link">
                <span class="nav-icon">🔑</span>
                Login
              </a>
              <a routerLink="/register" routerLinkActive="active" class="btn btn-primary">
                <span class="nav-icon">✨</span>
                Sign Up
              </a>
            </ng-container>

            <!-- Dark Mode Toggle -->
            <button
              (click)="toggleDarkMode()"
              class="theme-toggle"
              [attr.aria-label]="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
              [title]="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'">
              <span class="theme-icon" *ngIf="!isDarkMode">🌙</span>
              <span class="theme-icon" *ngIf="isDarkMode">☀️</span>
            </button>
          </nav>
        </div>
      </div>
    </header>
  `,
  styles: [`
    @keyframes slideInDown {
      from {
        opacity: 0;
        transform: translateY(-100%);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .animate-slideInDown {
      animation: slideInDown 0.5s ease-out;
    }

    .header {
      background: var(--bg-card);
      box-shadow: var(--shadow-md);
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 1000;
      backdrop-filter: blur(10px);
      transition: all 0.3s ease;
      border-bottom: 1px solid var(--border-color);
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 0;
    }

    .logo {
      display: flex;
      align-items: center;
      margin-right: 2rem;
    }

    .logo a {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      transition: transform 0.3s ease;
    }

    .logo a:hover {
      transform: scale(1.05);
    }

    .logo-icon {
      font-size: 2rem;
      animation: float 3s ease-in-out infinite;
    }

    .logo-text {
      font-size: 1.5rem;
      font-weight: 700;
      background: var(--gradient-primary);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      white-space: nowrap;
    }

    .nav {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .nav-link {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      color: var(--text-primary);
      padding: 0.5rem 1rem;
      border-radius: 8px;
      transition: all 0.3s ease;
      position: relative;
      overflow: hidden;
      line-height: 1;
    }

    .nav-link::before {
      content: '';
      position: absolute;
      bottom: 0;
      left: 0;
      width: 0;
      height: 2px;
      background: var(--primary);
      transition: width 0.3s ease;
    }

    .nav-link:hover::before {
      width: 100%;
    }

    .nav-link:hover {
      background: var(--bg-hover);
      transform: translateY(-2px);
    }

    .nav-link.active {
      background: var(--primary);
      color: white;
    }

    .nav-icon {
      font-size: 1.2rem;
      transition: transform 0.3s ease;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 1;
    }

    .nav-link:hover .nav-icon {
      transform: rotate(10deg) scale(1.1);
    }

    .icon-link {
      position: relative;
    }

    .badge {
      position: absolute;
      top: -8px;
      right: -8px;
      min-width: 20px;
      height: 20px;
      padding: 0 6px;
      background: var(--danger);
      color: white;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
      box-shadow: 0 2px 4px rgba(0,0,0,0.2);
    }

    .wishlist-badge {
      background: var(--warning);
    }

    .cart-badge {
      background: var(--danger);
    }

    .admin-link {
      color: var(--primary);
      font-weight: 600;
      border: 1px solid var(--primary);
    }

    .admin-link:hover {
      background: var(--primary);
      color: white;
    }

    .admin-link.active {
      background: var(--primary);
      color: white;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      line-height: 1;
    }

    .btn .nav-icon {
      font-size: 1.2rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 1;
    }

    .theme-toggle {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      background: var(--bg-hover);
      border: 2px solid var(--border-color);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.3s ease;
      position: relative;
      overflow: hidden;
    }

    .theme-toggle:hover {
      background: var(--primary-bg);
      border-color: var(--primary);
      transform: rotate(180deg);
    }

    .theme-icon {
      font-size: 1.5rem;
      animation: scaleIn 0.3s ease;
    }

    @keyframes scaleIn {
      from {
        transform: scale(0);
      }
      to {
        transform: scale(1);
      }
    }

    @keyframes float {
      0%, 100% {
        transform: translateY(0);
      }
      50% {
        transform: translateY(-5px);
      }
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        gap: 1rem;
      }

      .nav {
        flex-wrap: wrap;
        justify-content: center;
      }

      .nav-link {
        padding: 0.4rem 0.8rem;
        font-size: 0.9rem;
      }

      .logo-text {
        font-size: 1.25rem;
      }
    }
  `]
})
export class HeaderComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  cartItemCount = 0;
  wishlistItemCount = 0;
  isAdmin = false;
  isDarkMode = false;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    // Subscribe to theme changes
    this.themeService.isDarkMode$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isDark => {
        this.isDarkMode = isDark;
      });

    // Subscribe to auth changes
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        this.isAdmin = this.authService.isAdmin();

        if (user) {
          this.loadCart();
          this.loadWishlist();
        } else {
          // Reset counts for guest users
          this.cartItemCount = 0;
          this.wishlistItemCount = 0;
        }
      });

    // Subscribe to cart changes (only for logged in users)
    this.cartService.cart$
      .pipe(takeUntil(this.destroy$))
      .subscribe(cart => {
        if (this.currentUser) {
          this.cartItemCount = cart?.totalItems || 0;
        }
      });

    // Subscribe to wishlist changes (only for logged in users)
    this.wishlistService.wishlist$
      .pipe(takeUntil(this.destroy$))
      .subscribe(wishlist => {
        if (this.currentUser) {
          this.wishlistItemCount = wishlist?.itemCount || 0;
        }
      });
  }

  loadCart(): void {
    this.cartService.getCart()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: (err) => console.error('Failed to load cart:', err)
      });
  }

  loadWishlist(): void {
    this.wishlistService.getWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: (err) => console.error('Failed to load wishlist:', err)
      });
  }

  toggleDarkMode(): void {
    this.themeService.toggleDarkMode();
  }

  logout(): void {
    this.authService.logout();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
