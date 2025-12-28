import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../../../services/order.service';
import { OrderSummary, OrderStatus } from '../../../models/order.models';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container animate-fadeIn">
      <div class="page-header">
        <h1>My Orders</h1>
        <p class="subtitle">Track and view all your orders</p>
      </div>

      <div class="orders-container">
        <div class="order-card animate-slideInLeft" *ngFor="let order of orders; let i = index" [style.animation-delay]="i * 0.1 + 's'">
          <div class="order-header">
            <div class="order-info">
              <h3>Order #{{ order.orderNumber }}</h3>
              <span class="order-date">{{ order.orderDate | date: 'medium' }}</span>
            </div>
            <span [class]="getStatusClass(order.status)" class="status-badge">
              {{ getStatusName(order.status) }}
            </span>
          </div>

          <div class="order-details">
            <div class="detail-item">
              <span class="label">Total Amount:</span>
              <span class="value">\${{ order.totalAmount.toFixed(2) }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Items:</span>
              <span class="value">{{ order.totalItems || 'N/A' }}</span>
            </div>
          </div>

          <div class="order-actions">
            <a [routerLink]="['/orders', order.orderId]" class="btn btn-primary">
              View Details
            </a>
            <button class="btn btn-secondary" *ngIf="order.status === OrderStatus.Pending && cancellingOrderId !== order.orderId" (click)="showCancelConfirm(order.orderId)">
              Cancel Order
            </button>

            <!-- Inline Cancel Confirmation -->
            <div *ngIf="cancellingOrderId === order.orderId" class="cancel-confirm">
              <span>Cancel this order?</span>
              <button class="btn btn-danger btn-sm" (click)="confirmCancel(order.orderId)">Yes, Cancel</button>
              <button class="btn btn-secondary btn-sm" (click)="hideCancelConfirm()">No</button>
            </div>
          </div>
        </div>

        <div class="no-orders" *ngIf="orders.length === 0 && !loading">
          <div class="empty-state">
            <div class="empty-icon">📦</div>
            <h3>No orders yet</h3>
            <p>Start shopping to see your orders here!</p>
            <a routerLink="/products" class="btn btn-primary">Browse Products</a>
          </div>
        </div>

        <div class="loading" *ngIf="loading">
          <div class="spinner"></div>
          <p>Loading your orders...</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .page-header {
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 2px solid var(--border-color);
    }

    .page-header h1 {
      color: var(--text-primary);
      font-size: 32px;
      font-weight: 700;
      margin: 0 0 8px 0;
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 16px;
      margin: 0;
    }

    .orders-container {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .order-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 24px;
      box-shadow: var(--shadow-md);
      transition: all 0.3s ease;
    }

    .order-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-lg);
    }

    .order-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 20px;
      padding-bottom: 16px;
      border-bottom: 1px solid var(--border-color);
    }

    .order-info h3 {
      color: var(--text-primary);
      font-size: 20px;
      font-weight: 600;
      margin: 0 0 8px 0;
    }

    .order-date {
      color: var(--text-secondary);
      font-size: 14px;
    }

    .status-badge {
      padding: 6px 16px;
      border-radius: 20px;
      font-size: 13px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .status-pending {
      background: rgba(245, 158, 11, 0.1);
      color: var(--warning);
      border: 1px solid var(--warning);
    }

    .status-processing {
      background: rgba(59, 130, 246, 0.1);
      color: var(--info);
      border: 1px solid var(--info);
    }

    .status-shipped {
      background: rgba(99, 102, 241, 0.1);
      color: var(--primary);
      border: 1px solid var(--primary);
    }

    .status-delivered {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
      border: 1px solid var(--success);
    }

    .status-cancelled {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
      border: 1px solid var(--danger);
    }

    .order-details {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 20px;
    }

    .detail-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      background: var(--bg-secondary);
      border-radius: 8px;
    }

    .detail-item .label {
      color: var(--text-secondary);
      font-size: 14px;
    }

    .detail-item .value {
      color: var(--text-primary);
      font-size: 16px;
      font-weight: 600;
    }

    .order-actions {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      align-items: center;
    }

    .cancel-confirm {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      background: rgba(239, 68, 68, 0.1);
      border-radius: 8px;
      border: 1px solid var(--danger);
    }

    .cancel-confirm span {
      color: var(--danger);
      font-weight: 500;
      font-size: 14px;
    }

    .btn-sm {
      padding: 6px 12px;
      font-size: 12px;
    }

    .no-orders {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 400px;
    }

    .empty-state {
      text-align: center;
      max-width: 400px;
    }

    .empty-icon {
      font-size: 80px;
      margin-bottom: 20px;
      animation: float 3s ease-in-out infinite;
    }

    .empty-state h3 {
      color: var(--text-primary);
      font-size: 24px;
      margin-bottom: 12px;
    }

    .empty-state p {
      color: var(--text-secondary);
      font-size: 16px;
      margin-bottom: 24px;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      color: var(--text-secondary);
    }

    .spinner {
      border: 4px solid var(--border-color);
      border-top: 4px solid var(--primary);
      border-radius: 50%;
      width: 50px;
      height: 50px;
      animation: spin 1s linear infinite;
      margin-bottom: 20px;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-10px); }
    }

    .animate-fadeIn {
      animation: fadeIn 0.6s ease-out;
    }

    .animate-slideInLeft {
      animation: slideInLeft 0.6s ease-out forwards;
      opacity: 0;
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes slideInLeft {
      from {
        opacity: 0;
        transform: translateX(-50px);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }

    @media (max-width: 768px) {
      .container {
        padding: 15px;
      }

      .page-header h1 {
        font-size: 24px;
      }

      .order-header {
        flex-direction: column;
        gap: 12px;
      }

      .status-badge {
        align-self: flex-start;
      }

      .order-details {
        grid-template-columns: 1fr;
      }

      .order-actions {
        flex-direction: column;
      }

      .order-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class OrderListComponent implements OnInit, OnDestroy {
  orders: OrderSummary[] = [];
  loading = false;
  OrderStatus = OrderStatus; // Make enum available to template
  cancellingOrderId: number | null = null;
  private destroy$ = new Subject<void>();

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loading = true;
    this.orderService.getUserOrders()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (orders) => {
          this.orders = orders;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading orders:', error);
          this.loading = false;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getStatusClass(status: OrderStatus): string {
    const statusName = OrderStatus[status].toLowerCase();
    return `status-badge status-${statusName}`;
  }

  getStatusName(status: OrderStatus): string {
    return OrderStatus[status];
  }

  showCancelConfirm(orderId: number): void {
    this.cancellingOrderId = orderId;
  }

  hideCancelConfirm(): void {
    this.cancellingOrderId = null;
  }

  confirmCancel(orderId: number): void {
    this.orderService.cancelOrder(orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // Update the order status locally
          const order = this.orders.find(o => o.orderId === orderId);
          if (order) {
            order.status = OrderStatus.Cancelled;
          }
          this.cancellingOrderId = null;
        },
        error: (error) => {
          console.error('Error cancelling order:', error);
          this.cancellingOrderId = null;
        }
      });
  }
}
