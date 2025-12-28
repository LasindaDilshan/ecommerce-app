import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { GuestSessionService } from '../../services/guest-session.service';
import { TrustBadgesComponent } from '../shared/trust-badges/trust-badges.component';
import { ShippingAddress } from '../../models/order.models';
import { Cart } from '../../models/cart.models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, TrustBadgesComponent],
  template: `
    <div class="container">
      <h1>Checkout</h1>

      <app-trust-badges></app-trust-badges>

      <!-- Order Summary -->
      <div *ngIf="cart" class="order-summary">
        <h3>Order Summary</h3>
        <div class="summary-row">
          <span>Subtotal ({{ cart.totalItems }} items):</span>
          <span>\${{ cart.subTotal.toFixed(2) }}</span>
        </div>
        <div *ngIf="cart.discountAmount > 0" class="summary-row discount">
          <span>Discount ({{ cart.couponCode }}):</span>
          <span>-\${{ cart.discountAmount.toFixed(2) }}</span>
        </div>
        <div class="summary-row">
          <span>Shipping (estimated):</span>
          <span>\$10.00</span>
        </div>
        <div class="summary-row">
          <span>Tax (estimated):</span>
          <span>\${{ calculateTax().toFixed(2) }}</span>
        </div>
        <div class="summary-row total">
          <strong>Estimated Total:</strong>
          <strong>\${{ calculateTotal().toFixed(2) }}</strong>
        </div>
      </div>

      <form (ngSubmit)="placeOrder()" #checkoutForm="ngForm">
        <!-- Guest Information (only shown for guests) -->
        <div *ngIf="!isLoggedIn" class="guest-section">
          <h3>Contact Information</h3>
          <div class="form-group">
            <input type="email" class="form-control" placeholder="Email *" [(ngModel)]="guestEmail" name="guestEmail" required />
          </div>
          <div class="form-group">
            <input class="form-control" placeholder="First Name *" [(ngModel)]="guestFirstName" name="guestFirstName" required />
          </div>
          <div class="form-group">
            <input class="form-control" placeholder="Last Name *" [(ngModel)]="guestLastName" name="guestLastName" required />
          </div>
          <div class="form-group">
            <input class="form-control" placeholder="Phone Number *" [(ngModel)]="guestPhone" name="guestPhone" required />
          </div>
          <p class="info-message">✓ You can checkout as a guest. Save your order number and email to track your order later.</p>
        </div>

        <!-- Shipping Address -->
        <h3>Shipping Address</h3>

        <div class="form-group">
          <input class="form-control" placeholder="First Name *" [(ngModel)]="address.firstName" name="firstName" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="Last Name *" [(ngModel)]="address.lastName" name="lastName" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="Address *" [(ngModel)]="address.address" name="address" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="City *" [(ngModel)]="address.city" name="city" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="State *" [(ngModel)]="address.state" name="state" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="Zip Code *" [(ngModel)]="address.zipCode" name="zipCode" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="Country *" [(ngModel)]="address.country" name="country" required />
        </div>
        <div class="form-group">
          <input class="form-control" placeholder="Phone" [(ngModel)]="address.phone" name="phone" />
        </div>

        <button type="submit" class="btn btn-primary" [disabled]="!checkoutForm.valid || loading">
          {{ loading ? 'Processing...' : 'Place Order' }}
        </button>
      </form>
    </div>
  `,
  styles: [`
    .container {
      padding-top: 100px;
      padding-bottom: 40px;
    }

    h1 {
      color: var(--text-primary);
      margin-bottom: 30px;
      font-size: 2rem;
    }

    .order-summary {
      background: var(--bg-card);
      padding: 24px;
      border-radius: 12px;
      margin-bottom: 30px;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
    }

    .order-summary h3 {
      margin-bottom: 20px;
      color: var(--text-primary);
      border-bottom: 2px solid var(--border-color);
      padding-bottom: 12px;
      font-size: 1.25rem;
    }

    .summary-row {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-secondary);
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
      font-size: 1.25rem;
      color: var(--text-primary);
    }

    .guest-section {
      background: var(--bg-secondary);
      padding: 24px;
      border-radius: 12px;
      margin-bottom: 30px;
      border: 1px solid var(--border-color);
    }

    .guest-section h3 {
      color: var(--text-primary);
      margin-bottom: 20px;
    }

    .info-message {
      margin-top: 15px;
      padding: 12px 16px;
      background: rgba(16, 185, 129, 0.1);
      border: 1px solid var(--success);
      border-radius: 8px;
      color: var(--success);
      font-size: 14px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-control {
      width: 100%;
      padding: 12px 16px;
      font-size: 1rem;
      color: var(--text-primary);
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      transition: border-color 0.2s, box-shadow 0.2s;
    }

    .form-control:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-control::placeholder {
      color: var(--text-tertiary);
    }

    h3 {
      margin-bottom: 20px;
      color: var(--text-primary);
      font-size: 1.25rem;
    }

    .btn {
      width: 100%;
      padding: 14px 24px;
      font-size: 1.1rem;
      font-weight: 600;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s;
      margin-top: 10px;
    }

    .btn-primary {
      background: var(--primary);
      color: white;
      border: none;
    }

    .btn-primary:hover:not(:disabled) {
      background: var(--primary-dark);
      transform: translateY(-2px);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    form {
      background: var(--bg-card);
      padding: 24px;
      border-radius: 12px;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
    }

    @media (max-width: 768px) {
      .container {
        padding-top: 80px;
      }

      h1 {
        font-size: 1.5rem;
      }

      .order-summary,
      .guest-section,
      form {
        padding: 16px;
      }
    }
  `]
})
export class CheckoutComponent implements OnInit, OnDestroy {
  address: ShippingAddress = {
    firstName: '',
    lastName: '',
    address: '',
    city: '',
    state: '',
    zipCode: '',
    country: '',
    phone: ''
  };

  // Guest checkout fields
  guestEmail = '';
  guestFirstName = '';
  guestLastName = '';
  guestPhone = '';

  loading = false;
  isLoggedIn = false;
  guestOrderNumber = '';
  cart: Cart | null = null;
  errorMessage = '';
  successMessage = '';
  private destroy$ = new Subject<void>();

  constructor(
    private orderService: OrderService,
    private cartService: CartService,
    private authService: AuthService,
    private guestSessionService: GuestSessionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
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
          // Cart loading failed - redirect to cart page
          this.router.navigate(['/cart']);
        }
      });
  }

  calculateTax(): number {
    if (!this.cart) return 0;
    const discountedSubTotal = this.cart.subTotal - this.cart.discountAmount;
    return discountedSubTotal * 0.08; // 8% tax
  }

  calculateTotal(): number {
    if (!this.cart) return 0;
    const discountedSubTotal = this.cart.subTotal - this.cart.discountAmount;
    const shipping = 10.00;
    const tax = this.calculateTax();
    return discountedSubTotal + shipping + tax;
  }

  placeOrder(): void {
    this.loading = true;

    if (this.isLoggedIn) {
      this.placeUserOrder();
    } else {
      this.placeGuestOrder();
    }
  }

  private placeUserOrder(): void {
    this.orderService.createOrder({
      shippingAddress: this.address,
      paymentMethod: 'Stripe',
      couponCode: this.cart?.couponCode
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.cartService.clearCart().pipe(takeUntil(this.destroy$)).subscribe({
            error: () => { /* Cart clear failed - order still placed successfully */ }
          });
          this.router.navigate(['/orders', result.order.orderId]);
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Failed to place order';
        }
      });
  }

  private placeGuestOrder(): void {
    const sessionId = this.guestSessionService.getSessionId();

    this.orderService.createGuestOrder({
      sessionId,
      email: this.guestEmail,
      firstName: this.guestFirstName,
      lastName: this.guestLastName,
      phoneNumber: this.guestPhone,
      shippingAddress: this.address,
      couponCode: this.cart?.couponCode
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.guestOrderNumber = result.orderNumber;
          this.cartService.clearCart().pipe(takeUntil(this.destroy$)).subscribe({
            error: () => { /* Cart clear failed - order still placed successfully */ }
          });
          // Navigate to guest order tracking page with order info
          this.router.navigate(['/track-order'], {
            queryParams: {
              orderNumber: result.orderNumber,
              email: result.email
            }
          });
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Failed to place order';
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
