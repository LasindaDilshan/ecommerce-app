import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CartService } from '../../services/cart.service';
import { Cart } from '../../models/cart.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container">
      <h1>Shopping Cart</h1>

      <div *ngIf="cart && cart.items.length > 0">
        <div class="cart-item" *ngFor="let item of cart.items">
          <img [src]="getImageUrl(item.productImage)" alt="" />
          <div class="item-info">
            <h3>{{ item.productName }}</h3>
            <p>\${{ item.discountPrice || item.price }}</p>
          </div>
          <div class="item-quantity">
            <button (click)="updateQuantity(item.cartItemId, item.quantity - 1)" [disabled]="item.quantity <= 1">-</button>
            <span>{{ item.quantity }}</span>
            <button (click)="updateQuantity(item.cartItemId, item.quantity + 1)">+</button>
          </div>
          <p class="item-total">\${{ item.totalPrice }}</p>
          <button (click)="removeItem(item.cartItemId)" class="btn btn-danger">Remove</button>
        </div>

        <!-- Coupon Section -->
        <div class="coupon-section">
          <h3>Have a Coupon Code?</h3>
          <div *ngIf="!cart.couponCode" class="coupon-input">
            <input
              type="text"
              [(ngModel)]="couponCode"
              placeholder="Enter coupon code"
              class="form-control"
            />
            <button
              (click)="applyCoupon()"
              [disabled]="!couponCode || applyingCoupon"
              class="btn btn-primary"
            >
              {{ applyingCoupon ? 'Applying...' : 'Apply Coupon' }}
            </button>
          </div>

          <div *ngIf="cart.couponCode" class="coupon-applied">
            <span class="success-badge">✓ Coupon Applied: {{ cart.couponCode }}</span>
            <button (click)="removeCoupon()" class="btn btn-secondary btn-sm">Remove</button>
          </div>

          <p *ngIf="couponError" class="error-message">{{ couponError }}</p>
          <p *ngIf="couponSuccess" class="success-message">{{ couponSuccess }}</p>
        </div>

        <div class="cart-summary">
          <div class="summary-row">
            <span>Subtotal:</span>
            <span>\${{ cart.subTotal.toFixed(2) }}</span>
          </div>
          <div *ngIf="cart.discountAmount > 0" class="summary-row discount">
            <span>Discount ({{ cart.couponCode }}):</span>
            <span>-\${{ cart.discountAmount.toFixed(2) }}</span>
          </div>
          <div class="summary-row total">
            <h3>Total:</h3>
            <h3>\${{ cart.finalTotal.toFixed(2) }}</h3>
          </div>
          <a routerLink="/checkout" class="btn btn-primary">Proceed to Checkout</a>
        </div>
      </div>

      <div *ngIf="!cart || cart.items.length === 0">
        <p>Your cart is empty</p>
        <a routerLink="/products" class="btn btn-primary">Continue Shopping</a>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding-top: 100px;
      padding-bottom: 40px;
      max-width: 900px;
    }

    h1 {
      color: var(--text-primary);
      margin-bottom: 30px;
      font-size: 2rem;
    }

    .cart-item {
      display: grid;
      grid-template-columns: 100px 1fr auto auto auto;
      align-items: center;
      gap: 20px;
      padding: 20px;
      background: var(--bg-card);
      margin-bottom: 15px;
      border-radius: 12px;
      box-shadow: var(--shadow-sm);
      border: 1px solid var(--border-color);
      transition: transform 0.2s;
    }

    .cart-item:hover {
      transform: translateY(-2px);
    }

    .cart-item img {
      width: 100px;
      height: 100px;
      object-fit: cover;
      border-radius: 8px;
    }

    .item-info {
      min-width: 0;
    }

    .item-info h3 {
      margin: 0 0 8px 0;
      color: var(--text-primary);
      font-size: 1.1rem;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .item-info p {
      margin: 0;
      color: var(--primary);
      font-weight: 600;
      font-size: 1rem;
    }

    .item-quantity {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .item-quantity button {
      width: 36px;
      height: 36px;
      padding: 0;
      background: var(--bg-secondary);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      color: var(--text-primary);
      cursor: pointer;
      font-size: 1.2rem;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s;
    }

    .item-quantity button:hover:not(:disabled) {
      background: var(--primary);
      color: white;
      border-color: var(--primary);
    }

    .item-quantity button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .item-quantity span {
      color: var(--text-primary);
      font-weight: 600;
      min-width: 40px;
      text-align: center;
      font-size: 1.1rem;
    }

    .item-total {
      color: var(--text-primary);
      font-weight: 700;
      font-size: 1.25rem;
      margin: 0;
      min-width: 100px;
      text-align: right;
    }

    .btn-danger {
      background: var(--danger);
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-danger:hover {
      opacity: 0.9;
      transform: translateY(-1px);
    }

    .coupon-section {
      background: var(--bg-card);
      padding: 24px;
      border-radius: 12px;
      margin-bottom: 20px;
      box-shadow: var(--shadow-sm);
      border: 1px solid var(--border-color);
    }

    .coupon-section h3 {
      margin-bottom: 15px;
      color: var(--text-primary);
      font-size: 1.1rem;
    }

    .coupon-input {
      display: flex;
      gap: 10px;
    }

    .coupon-input input {
      flex: 1;
      padding: 12px 16px;
      border: 1px solid var(--border-color);
      border-radius: 8px;
      color: var(--text-primary);
      background: var(--bg-secondary);
      font-size: 1rem;
      transition: border-color 0.2s;
    }

    .coupon-input input:focus {
      outline: none;
      border-color: var(--primary);
    }

    .coupon-input input::placeholder {
      color: var(--text-tertiary);
    }

    .coupon-input .btn-primary {
      padding: 12px 20px;
      white-space: nowrap;
    }

    .coupon-applied {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
    }

    .success-badge {
      background: rgba(16, 185, 129, 0.15);
      color: var(--success);
      padding: 10px 16px;
      border-radius: 8px;
      font-weight: 600;
      border: 1px solid var(--success);
    }

    .btn-sm {
      padding: 8px 12px;
      font-size: 14px;
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-secondary:hover {
      background: var(--bg-hover);
    }

    .error-message {
      margin-top: 12px;
      padding: 12px 16px;
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
      border-radius: 8px;
      border: 1px solid var(--danger);
    }

    .success-message {
      margin-top: 12px;
      padding: 12px 16px;
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
      border-radius: 8px;
      border: 1px solid var(--success);
    }

    .cart-summary {
      background: var(--bg-card);
      padding: 24px;
      border-radius: 12px;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
    }

    .summary-row {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-secondary);
      font-size: 1rem;
    }

    .summary-row.discount {
      color: var(--success);
      font-weight: 600;
    }

    .summary-row.total {
      border-bottom: none;
      border-top: 2px solid var(--primary);
      margin-top: 12px;
      padding-top: 16px;
    }

    .summary-row.total h3 {
      margin: 0;
      color: var(--text-primary);
      font-size: 1.25rem;
    }

    .cart-summary .btn-primary {
      width: 100%;
      margin-top: 20px;
      padding: 14px 24px;
      font-size: 1.1rem;
      font-weight: 600;
      border-radius: 8px;
      background: var(--primary);
      color: white;
      border: none;
      cursor: pointer;
      transition: all 0.2s;
      text-decoration: none;
      display: block;
      text-align: center;
    }

    .cart-summary .btn-primary:hover {
      background: var(--primary-dark);
      transform: translateY(-2px);
    }

    .btn-primary {
      background: var(--primary);
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
      text-decoration: none;
    }

    .btn-primary:hover {
      background: var(--primary-dark);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    /* Empty cart state */
    div > p {
      color: var(--text-secondary);
      font-size: 1.1rem;
      margin-bottom: 20px;
    }

    @media (max-width: 768px) {
      .container {
        padding-top: 80px;
      }

      h1 {
        font-size: 1.5rem;
      }

      .cart-item {
        grid-template-columns: 80px 1fr;
        grid-template-rows: auto auto auto;
        gap: 12px;
      }

      .cart-item img {
        width: 80px;
        height: 80px;
        grid-row: 1 / 3;
      }

      .item-info {
        grid-column: 2;
      }

      .item-info h3 {
        white-space: normal;
      }

      .item-quantity {
        grid-column: 1 / 3;
        justify-content: center;
      }

      .item-total {
        grid-column: 1 / 3;
        text-align: center;
      }

      .btn-danger {
        grid-column: 1 / 3;
        width: 100%;
        justify-content: center;
      }

      .coupon-input {
        flex-direction: column;
      }

      .coupon-applied {
        flex-direction: column;
        align-items: flex-start;
      }
    }
  `]
})
export class CartComponent implements OnInit, OnDestroy {
  cart: Cart | null = null;
  couponCode = '';
  applyingCoupon = false;
  couponError = '';
  couponSuccess = '';
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  constructor(
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.cartService.getCart()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (cart) => {
          this.cart = cart;
        },
        error: () => {
          // Cart loading failed - show empty cart
          this.cart = null;
        }
      });
  }

  updateQuantity(cartItemId: number, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateCartItem(cartItemId, { quantity })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadCart();
        },
        error: () => {
          // Reload cart to show current state on failure
          this.loadCart();
        }
      });
  }

  removeItem(cartItemId: number): void {
    this.cartService.removeFromCart(cartItemId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadCart();
        },
        error: () => {
          // Reload cart to show current state on failure
          this.loadCart();
        }
      });
  }

  applyCoupon(): void {
    if (!this.couponCode.trim()) return;

    this.applyingCoupon = true;
    this.couponError = '';
    this.couponSuccess = '';

    this.cartService.applyCoupon(this.couponCode.trim())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (cart) => {
          this.cart = cart;
          this.couponSuccess = `Coupon applied! You saved $${cart.discountAmount.toFixed(2)}`;
          this.couponCode = '';
          this.applyingCoupon = false;
          this.timeoutIds.push(setTimeout(() => this.couponSuccess = '', 5000));
        },
        error: (error) => {
          this.applyingCoupon = false;
          this.couponError = error.error?.message || 'Failed to apply coupon. Please check the code and try again.';
          this.timeoutIds.push(setTimeout(() => this.couponError = '', 5000));
        }
      });
  }

  removeCoupon(): void {
    this.cartService.removeCoupon()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (cart) => {
          this.cart = cart;
          this.couponSuccess = 'Coupon removed';
          this.timeoutIds.push(setTimeout(() => this.couponSuccess = '', 3000));
        },
        error: (error) => {
          this.couponError = error.error?.message || 'Failed to remove coupon';
          this.timeoutIds.push(setTimeout(() => this.couponError = '', 5000));
        }
      });
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/100x100/CCCCCC/FFFFFF?text=No+Image';
    }
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timeoutIds.forEach(id => clearTimeout(id));
  }
}
