import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../../../services/order.service';
import { Order, OrderStatus, PaymentStatus } from '../../../models/order.models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="order-detail-page" *ngIf="order">
      <div class="container">
        <!-- Back Button -->
        <a routerLink="/orders" class="back-link">
          <span class="back-icon">&#8592;</span> Back to Orders
        </a>

        <!-- Order Header -->
        <div class="order-header">
          <div class="order-title">
            <h1>Order Details</h1>
            <div class="order-number">
              <span class="label">Order Number:</span>
              <span class="value">#{{ order.orderNumber || 'N/A' }}</span>
            </div>
          </div>
          <div class="order-badges">
            <div class="badge-group">
              <span class="badge-label">Order:</span>
              <span class="status-badge" [class]="getStatusClass(order.status)">
                {{ getStatusLabel(order.status) }}
              </span>
            </div>
            <div class="badge-group">
              <span class="badge-label">Payment:</span>
              <span class="payment-badge" [class]="getPaymentStatusClass(order.paymentStatus)">
                {{ getPaymentStatusLabel(order.paymentStatus) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Order Summary Cards -->
        <div class="summary-grid">
          <div class="summary-card">
            <div class="card-icon">&#128197;</div>
            <div class="card-content">
              <span class="card-label">Order Date</span>
              <span class="card-value">{{ order.orderDate | date:'MMM d, yyyy' }}</span>
              <span class="card-sub">{{ order.orderDate | date:'h:mm a' }}</span>
            </div>
          </div>
          <div class="summary-card">
            <div class="card-icon">&#128230;</div>
            <div class="card-content">
              <span class="card-label">Total Items</span>
              <span class="card-value">{{ getTotalItems() }}</span>
              <span class="card-sub">{{ order.items.length }} product(s)</span>
            </div>
          </div>
          <div class="summary-card highlight">
            <div class="card-icon">&#128176;</div>
            <div class="card-content">
              <span class="card-label">Total Amount</span>
              <span class="card-value">\${{ order.totalAmount.toFixed(2) }}</span>
              <span class="card-sub">Including tax & shipping</span>
            </div>
          </div>
        </div>

        <!-- Main Content Grid -->
        <div class="content-grid">
          <!-- Order Items Section -->
          <div class="items-section">
            <h2 class="section-title">
              <span class="title-icon">&#128722;</span>
              Order Items
            </h2>
            <div class="items-list">
              <div class="item-card" *ngFor="let item of order.items">
                <div class="item-image">
                  <img [src]="getImageUrl(item.productImage)" alt="">
                </div>
                <div class="item-details">
                  <h3 class="item-name">{{ item.productName }}</h3>
                  <div class="item-meta">
                    <span class="item-price">Unit Price: \${{ item.unitPrice.toFixed(2) }}</span>
                    <span class="item-quantity">Qty: {{ item.quantity }}</span>
                  </div>
                </div>
                <div class="item-total">
                  <span class="total-label">Subtotal</span>
                  <span class="total-value">\${{ item.totalPrice.toFixed(2) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Shipping Address Section -->
          <div class="shipping-section">
            <h2 class="section-title">
              <span class="title-icon">&#128205;</span>
              Shipping Address
            </h2>
            <div class="address-card">
              <div class="address-name">
                {{ order.shippingAddress.firstName }} {{ order.shippingAddress.lastName }}
              </div>
              <div class="address-line">{{ order.shippingAddress.address }}</div>
              <div class="address-line">
                {{ order.shippingAddress.city }}, {{ order.shippingAddress.state }} {{ order.shippingAddress.zipCode }}
              </div>
              <div class="address-line">{{ order.shippingAddress.country }}</div>
              <div class="address-phone" *ngIf="order.shippingAddress.phone">
                <span class="phone-icon">&#128222;</span>
                {{ order.shippingAddress.phone }}
              </div>
            </div>

            <!-- Order Timeline -->
            <h2 class="section-title timeline-title">
              <span class="title-icon">&#128337;</span>
              Order Timeline
            </h2>
            <!-- Normal Timeline (for non-cancelled orders) -->
            <div class="timeline" *ngIf="order.status !== 4">
              <div class="timeline-item" [class.active]="isStatusReached(0)" [class.current]="order.status === 0">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Order Placed</span>
                </div>
              </div>
              <div class="timeline-item" [class.active]="isStatusReached(1)" [class.current]="order.status === 1">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Processing</span>
                </div>
              </div>
              <div class="timeline-item" [class.active]="isStatusReached(2)" [class.current]="order.status === 2">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Shipped</span>
                </div>
              </div>
              <div class="timeline-item" [class.active]="isStatusReached(3)" [class.current]="order.status === 3">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Delivered</span>
                </div>
              </div>
            </div>
            <!-- Cancelled Timeline -->
            <div class="timeline" *ngIf="order.status === 4">
              <div class="timeline-item active">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Order Placed</span>
                </div>
              </div>
              <div class="timeline-item active cancelled current">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Order Cancelled</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div class="loading-container" *ngIf="!order && loading">
      <div class="spinner"></div>
      <p>Loading order details...</p>
    </div>

    <!-- Error State -->
    <div class="error-container" *ngIf="!order && !loading && error">
      <div class="error-icon">&#9888;</div>
      <h2>Order Not Found</h2>
      <p>{{ error }}</p>
      <a routerLink="/orders" class="btn btn-primary">Back to Orders</a>
    </div>
  `,
  styles: [`
    .order-detail-page {
      min-height: 100vh;
      background: var(--bg-secondary);
      padding: 2rem 0;
    }

    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 1rem;
    }

    .back-link {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      color: var(--primary);
      text-decoration: none;
      font-weight: 500;
      margin-bottom: 1.5rem;
      transition: transform 0.2s;
    }

    .back-link:hover {
      transform: translateX(-5px);
    }

    .back-icon {
      font-size: 1.2rem;
    }

    /* Order Header */
    .order-header {
      background: var(--bg-card);
      border-radius: 16px;
      padding: 2rem;
      margin-bottom: 1.5rem;
      box-shadow: var(--shadow-md);
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      border: 1px solid var(--border-color);
    }

    .order-title h1 {
      margin: 0 0 0.5rem 0;
      font-size: 1.75rem;
      color: var(--text-primary);
    }

    .order-number {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .order-number .label {
      color: var(--text-tertiary);
      font-size: 0.9rem;
    }

    .order-number .value {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--primary);
      font-family: monospace;
    }

    .order-badges {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .badge-group {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .badge-label {
      font-size: 0.85rem;
      color: var(--text-tertiary);
      font-weight: 500;
    }

    .status-badge, .payment-badge {
      padding: 0.5rem 1rem;
      border-radius: 20px;
      font-size: 0.85rem;
      font-weight: 600;
      text-transform: uppercase;
    }

    .status-pending { background: #fef3c7; color: #92400e; }
    .status-processing { background: #dbeafe; color: #1e40af; }
    .status-shipped { background: #e0e7ff; color: #4338ca; }
    .status-delivered { background: #d1fae5; color: #065f46; }
    .status-cancelled { background: #fee2e2; color: #991b1b; }

    .payment-pending { background: #fef3c7; color: #92400e; }
    .payment-paid { background: #d1fae5; color: #065f46; }
    .payment-failed { background: #fee2e2; color: #991b1b; }
    .payment-refunded { background: #e5e7eb; color: #374151; }

    /* Summary Cards */
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .summary-card {
      background: var(--bg-card);
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: var(--shadow-sm);
      display: flex;
      align-items: center;
      gap: 1rem;
      transition: transform 0.2s;
      border: 1px solid var(--border-color);
    }

    .summary-card:hover {
      transform: translateY(-2px);
    }

    .summary-card.highlight {
      background: linear-gradient(135deg, var(--primary) 0%, #764ba2 100%);
      color: white;
      border: none;
    }

    .summary-card.highlight .card-label,
    .summary-card.highlight .card-sub {
      color: rgba(255, 255, 255, 0.8);
    }

    .card-icon {
      font-size: 2rem;
    }

    .card-content {
      display: flex;
      flex-direction: column;
    }

    .card-label {
      font-size: 0.8rem;
      color: var(--text-tertiary);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .card-value {
      font-size: 1.5rem;
      font-weight: 700;
      line-height: 1.2;
      color: var(--text-primary);
    }

    .card-sub {
      font-size: 0.75rem;
      color: var(--text-tertiary);
    }

    /* Content Grid */
    .content-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 1.5rem;
    }

    /* Sections */
    .items-section, .shipping-section {
      background: var(--bg-card);
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin: 0 0 1.5rem 0;
      font-size: 1.25rem;
      color: var(--text-primary);
    }

    .title-icon {
      font-size: 1.5rem;
    }

    .timeline-title {
      margin-top: 2rem;
    }

    /* Items List */
    .items-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .item-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      background: var(--bg-secondary);
      border-radius: 12px;
      transition: background 0.2s;
      border: 1px solid var(--border-color);
    }

    .item-card:hover {
      background: var(--bg-hover);
    }

    .item-image {
      width: 80px;
      height: 80px;
      border-radius: 8px;
      overflow: hidden;
      flex-shrink: 0;
    }

    .item-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .item-details {
      flex: 1;
    }

    .item-name {
      margin: 0 0 0.5rem 0;
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .item-meta {
      display: flex;
      gap: 1rem;
      font-size: 0.85rem;
      color: var(--text-tertiary);
    }

    .item-total {
      text-align: right;
    }

    .total-label {
      display: block;
      font-size: 0.75rem;
      color: var(--text-tertiary);
      text-transform: uppercase;
    }

    .total-value {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--primary);
    }

    /* Address Card */
    .address-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 1.25rem;
      border: 1px solid var(--border-color);
    }

    .address-name {
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.75rem;
    }

    .address-line {
      color: var(--text-secondary);
      margin-bottom: 0.25rem;
      line-height: 1.5;
    }

    .address-phone {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-top: 0.75rem;
      color: var(--primary);
      font-weight: 500;
    }

    /* Timeline */
    .timeline {
      position: relative;
      padding-left: 2rem;
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 8px;
      top: 0;
      bottom: 0;
      width: 2px;
      background: var(--border-color);
    }

    .timeline-item {
      position: relative;
      padding-bottom: 1.5rem;
    }

    .timeline-item:last-child {
      padding-bottom: 0;
    }

    .timeline-dot {
      position: absolute;
      left: -2rem;
      top: 0;
      width: 18px;
      height: 18px;
      border-radius: 50%;
      background: var(--bg-tertiary);
      border: 3px solid var(--bg-card);
      box-shadow: 0 0 0 2px var(--border-color);
    }

    .timeline-item.active .timeline-dot {
      background: var(--primary);
      box-shadow: 0 0 0 2px var(--primary);
    }

    .timeline-item.current .timeline-dot {
      background: var(--primary);
      box-shadow: 0 0 0 2px var(--primary), 0 0 0 6px rgba(99, 102, 241, 0.3);
      animation: pulse-dot 2s infinite;
    }

    @keyframes pulse-dot {
      0%, 100% { box-shadow: 0 0 0 2px var(--primary), 0 0 0 6px rgba(99, 102, 241, 0.3); }
      50% { box-shadow: 0 0 0 2px var(--primary), 0 0 0 10px rgba(99, 102, 241, 0.1); }
    }

    .timeline-label {
      font-size: 0.9rem;
      color: var(--text-tertiary);
    }

    .timeline-item.active .timeline-label {
      color: var(--text-primary);
      font-weight: 500;
    }

    .timeline-item.cancelled .timeline-dot {
      background: var(--danger);
      box-shadow: 0 0 0 2px var(--danger);
    }

    .timeline-item.cancelled.current .timeline-dot {
      box-shadow: 0 0 0 2px var(--danger), 0 0 0 6px rgba(239, 68, 68, 0.3);
      animation: pulse-cancelled 2s infinite;
    }

    @keyframes pulse-cancelled {
      0%, 100% { box-shadow: 0 0 0 2px var(--danger), 0 0 0 6px rgba(239, 68, 68, 0.3); }
      50% { box-shadow: 0 0 0 2px var(--danger), 0 0 0 10px rgba(239, 68, 68, 0.1); }
    }

    .timeline-item.cancelled .timeline-label {
      color: var(--danger);
      font-weight: 600;
    }

    /* Loading & Error States */
    .loading-container, .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 50vh;
      text-align: center;
      padding: 2rem;
    }

    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid var(--border-color);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .error-icon {
      font-size: 4rem;
      color: var(--danger);
      margin-bottom: 1rem;
    }

    .error-container h2 {
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .error-container p {
      color: var(--text-tertiary);
      margin-bottom: 1.5rem;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-weight: 600;
      text-decoration: none;
      transition: all 0.2s;
    }

    .btn-primary {
      background: var(--primary);
      color: white;
    }

    .btn-primary:hover {
      background: var(--primary-dark);
      transform: translateY(-2px);
    }

    /* Responsive Design */
    @media (max-width: 1024px) {
      .content-grid {
        grid-template-columns: 1fr;
      }

      .shipping-section {
        order: -1;
      }
    }

    @media (max-width: 768px) {
      .order-detail-page {
        padding: 1rem 0;
      }

      .order-header {
        flex-direction: column;
        align-items: flex-start;
        padding: 1.5rem;
      }

      .order-title h1 {
        font-size: 1.5rem;
      }

      .summary-grid {
        grid-template-columns: 1fr;
      }

      .item-card {
        flex-direction: column;
        text-align: center;
      }

      .item-image {
        width: 100px;
        height: 100px;
      }

      .item-meta {
        justify-content: center;
      }

      .item-total {
        text-align: center;
        margin-top: 0.5rem;
      }
    }

    @media (max-width: 480px) {
      .order-number {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.25rem;
      }

      .order-badges {
        width: 100%;
      }

      .status-badge, .payment-badge {
        flex: 1;
        text-align: center;
      }
    }
  `]
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  order: Order | null = null;
  loading = true;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getOrderById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (order) => {
          this.order = order;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load order details. Please try again.';
          this.loading = false;
          console.error('Error loading order:', err);
        }
      });
  }

  getTotalItems(): number {
    if (!this.order) return 0;
    return this.order.items.reduce((sum, item) => sum + item.quantity, 0);
  }

  getStatusLabel(status: OrderStatus): string {
    const labels: { [key: number]: string } = {
      0: 'Pending',
      1: 'Processing',
      2: 'Shipped',
      3: 'Delivered',
      4: 'Cancelled'
    };
    return labels[status] || 'Unknown';
  }

  getStatusClass(status: OrderStatus): string {
    const classes: { [key: number]: string } = {
      0: 'status-pending',
      1: 'status-processing',
      2: 'status-shipped',
      3: 'status-delivered',
      4: 'status-cancelled'
    };
    return classes[status] || '';
  }

  getPaymentStatusLabel(status: PaymentStatus): string {
    const labels: { [key: number]: string } = {
      0: 'Pending',
      1: 'Paid',
      2: 'Failed',
      3: 'Refunded'
    };
    return labels[status] || 'Unknown';
  }

  getPaymentStatusClass(status: PaymentStatus): string {
    const classes: { [key: number]: string } = {
      0: 'payment-pending',
      1: 'payment-paid',
      2: 'payment-failed',
      3: 'payment-refunded'
    };
    return classes[status] || '';
  }

  isStatusReached(statusValue: number): boolean {
    if (!this.order) return false;
    // For cancelled orders, only show "Order Placed" as active
    if (this.order.status === 4) return statusValue === 0;
    return this.order.status >= statusValue;
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/80x80/CCCCCC/FFFFFF?text=No+Image';
    }

    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }

    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
