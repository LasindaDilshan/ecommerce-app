import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../../../services/order.service';
import { Order, OrderStatus, PaymentStatus } from '../../../models/order.models';

@Component({
  selector: 'app-order-management',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="admin-container">
      <div class="page-header">
        <h1>Order Management</h1>
        <p class="subtitle">Manage and track all customer orders</p>
      </div>

      <!-- Stats Cards -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon pending">&#128337;</div>
          <div class="stat-info">
            <span class="stat-value">{{ getPendingCount() }}</span>
            <span class="stat-label">Pending</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon processing">&#9881;</div>
          <div class="stat-info">
            <span class="stat-value">{{ getProcessingCount() }}</span>
            <span class="stat-label">Processing</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon shipped">&#128666;</div>
          <div class="stat-info">
            <span class="stat-value">{{ getShippedCount() }}</span>
            <span class="stat-label">Shipped</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon delivered">&#9989;</div>
          <div class="stat-info">
            <span class="stat-value">{{ getDeliveredCount() }}</span>
            <span class="stat-label">Delivered</span>
          </div>
        </div>
      </div>

      <!-- Orders Table -->
      <div class="table-container">
        <div class="table-header">
          <h2>All Orders</h2>
          <div class="table-actions">
            <select [(ngModel)]="statusFilter" (change)="filterOrders()" class="filter-select">
              <option value="">All Statuses</option>
              <option value="0">Pending</option>
              <option value="1">Processing</option>
              <option value="2">Shipped</option>
              <option value="3">Delivered</option>
              <option value="4">Cancelled</option>
            </select>
          </div>
        </div>

        <div class="orders-list" *ngIf="filteredOrders.length > 0">
          <div class="order-card" *ngFor="let order of filteredOrders">
            <div class="order-main">
              <div class="order-info">
                <div class="order-number">#{{ order.orderNumber }}</div>
                <div class="order-date">{{ order.orderDate | date:'MMM d, yyyy h:mm a' }}</div>
              </div>
              <div class="order-customer">
                <span class="customer-name">{{ order.shippingAddress?.firstName }} {{ order.shippingAddress?.lastName }}</span>
                <span class="customer-location">{{ order.shippingAddress?.city }}, {{ order.shippingAddress?.state }}</span>
              </div>
              <div class="order-amount">
                <span class="amount-value">\${{ order.totalAmount.toFixed(2) }}</span>
                <span class="items-count">{{ order.items?.length || 0 }} item(s)</span>
              </div>
              <div class="order-status">
                <div class="status-group">
                  <span class="status-label">Order:</span>
                  <span class="status-badge" [class]="getStatusClass(order.status)">
                    {{ getStatusLabel(order.status) }}
                  </span>
                </div>
                <div class="status-group">
                  <span class="status-label">Payment:</span>
                  <span class="payment-badge" [class]="getPaymentClass(order.paymentStatus)">
                    {{ getPaymentLabel(order.paymentStatus) }}
                  </span>
                </div>
              </div>
            </div>
            <div class="order-actions">
              <a [routerLink]="['/orders', order.orderId]" class="btn btn-view">
                <span>&#128065;</span> View
              </a>
              <div class="status-dropdown" *ngIf="order.status !== 4 && order.status !== 3">
                <select
                  [value]="order.status"
                  (change)="updateStatus(order.orderId, $event)"
                  class="status-select">
                  <option value="0">Pending</option>
                  <option value="1">Processing</option>
                  <option value="2">Shipped</option>
                  <option value="3">Delivered</option>
                </select>
              </div>
              <button
                *ngIf="order.status !== 4 && order.status !== 3"
                (click)="cancelOrder(order.orderId)"
                class="btn btn-cancel">
                <span>&#10060;</span> Cancel
              </button>
            </div>
          </div>
        </div>

        <div class="empty-state" *ngIf="filteredOrders.length === 0">
          <div class="empty-icon">&#128230;</div>
          <p>No orders found</p>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading orders...</p>
      </div>
    </div>
  `,
  styles: [`
    .admin-container {
      padding: 100px 20px 40px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .page-header {
      margin-bottom: 30px;
    }

    .page-header h1 {
      color: var(--text-primary);
      font-size: 2rem;
      margin: 0 0 8px 0;
    }

    .subtitle {
      color: var(--text-tertiary);
      margin: 0;
    }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 20px;
      margin-bottom: 30px;
    }

    .stat-card {
      background: var(--bg-card);
      border-radius: 12px;
      padding: 20px;
      display: flex;
      align-items: center;
      gap: 16px;
      box-shadow: var(--shadow-sm);
      border: 1px solid var(--border-color);
    }

    .stat-icon {
      width: 50px;
      height: 50px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }

    .stat-icon.pending { background: rgba(245, 158, 11, 0.15); }
    .stat-icon.processing { background: rgba(59, 130, 246, 0.15); }
    .stat-icon.shipped { background: rgba(139, 92, 246, 0.15); }
    .stat-icon.delivered { background: rgba(16, 185, 129, 0.15); }

    .stat-info {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--text-tertiary);
    }

    /* Table Container */
    .table-container {
      background: var(--bg-card);
      border-radius: 12px;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
      overflow: hidden;
    }

    .table-header {
      padding: 20px 24px;
      border-bottom: 1px solid var(--border-color);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .table-header h2 {
      margin: 0;
      color: var(--text-primary);
      font-size: 1.25rem;
    }

    .filter-select {
      padding: 8px 12px;
      border-radius: 8px;
      border: 1px solid var(--border-color);
      background: var(--bg-secondary);
      color: var(--text-primary);
      font-size: 0.875rem;
      cursor: pointer;
    }

    /* Orders List */
    .orders-list {
      padding: 16px;
    }

    .order-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 20px;
      margin-bottom: 12px;
      border: 1px solid var(--border-color);
      transition: all 0.2s;
    }

    .order-card:hover {
      border-color: var(--primary);
      transform: translateY(-2px);
    }

    .order-card:last-child {
      margin-bottom: 0;
    }

    .order-main {
      display: grid;
      grid-template-columns: 1.5fr 1fr 1fr 1fr;
      gap: 20px;
      align-items: center;
      margin-bottom: 16px;
    }

    .order-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .order-number {
      font-weight: 700;
      color: var(--primary);
      font-family: monospace;
      font-size: 1rem;
    }

    .order-date {
      font-size: 0.8rem;
      color: var(--text-tertiary);
    }

    .order-customer {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .customer-name {
      font-weight: 600;
      color: var(--text-primary);
    }

    .customer-location {
      font-size: 0.8rem;
      color: var(--text-tertiary);
    }

    .order-amount {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .amount-value {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .items-count {
      font-size: 0.8rem;
      color: var(--text-tertiary);
    }

    .order-status {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .status-group {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .status-label {
      font-size: 0.75rem;
      color: var(--text-tertiary);
      font-weight: 500;
      min-width: 55px;
    }

    .status-badge, .payment-badge {
      padding: 4px 10px;
      border-radius: 20px;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
      text-align: center;
    }

    .status-pending { background: #fef3c7; color: #92400e; }
    .status-processing { background: #dbeafe; color: #1e40af; }
    .status-shipped { background: #e0e7ff; color: #4338ca; }
    .status-delivered { background: #d1fae5; color: #065f46; }
    .status-cancelled { background: #fee2e2; color: #991b1b; }

    .payment-pending { background: rgba(245, 158, 11, 0.15); color: #d97706; }
    .payment-paid { background: rgba(16, 185, 129, 0.15); color: #059669; }
    .payment-failed { background: rgba(239, 68, 68, 0.15); color: #dc2626; }
    .payment-refunded { background: rgba(107, 114, 128, 0.15); color: #6b7280; }

    /* Order Actions */
    .order-actions {
      display: flex;
      gap: 10px;
      padding-top: 16px;
      border-top: 1px solid var(--border-color);
    }

    .btn {
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      text-decoration: none;
    }

    .btn-view {
      background: var(--primary);
      color: white;
      border: none;
    }

    .btn-view:hover {
      background: var(--primary-dark);
    }

    .btn-cancel {
      background: transparent;
      color: var(--danger);
      border: 1px solid var(--danger);
    }

    .btn-cancel:hover {
      background: var(--danger);
      color: white;
    }

    .status-select {
      padding: 8px 12px;
      border-radius: 8px;
      border: 1px solid var(--border-color);
      background: var(--bg-card);
      color: var(--text-primary);
      font-size: 0.875rem;
      cursor: pointer;
    }

    .status-select:focus {
      outline: none;
      border-color: var(--primary);
    }

    /* Empty State */
    .empty-state {
      padding: 60px 20px;
      text-align: center;
    }

    .empty-icon {
      font-size: 4rem;
      margin-bottom: 16px;
    }

    .empty-state p {
      color: var(--text-tertiary);
      margin: 0;
    }

    /* Loading */
    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 60px 20px;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .loading p {
      margin-top: 16px;
      color: var(--text-tertiary);
    }

    /* Responsive */
    @media (max-width: 1200px) {
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }

      .order-main {
        grid-template-columns: 1fr 1fr;
      }
    }

    @media (max-width: 768px) {
      .admin-container {
        padding: 80px 16px 40px;
      }

      .stats-grid {
        grid-template-columns: 1fr 1fr;
        gap: 12px;
      }

      .stat-card {
        padding: 16px;
      }

      .order-main {
        grid-template-columns: 1fr;
        gap: 12px;
      }

      .order-actions {
        flex-wrap: wrap;
      }

      .table-header {
        flex-direction: column;
        gap: 12px;
        align-items: flex-start;
      }
    }
  `]
})
export class OrderManagementComponent implements OnInit, OnDestroy {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  statusFilter = '';
  loading = true;
  private destroy$ = new Subject<void>();

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getAllOrders(1, 100)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.orders = result.items;
          this.filteredOrders = result.items;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  filterOrders(): void {
    if (!this.statusFilter) {
      this.filteredOrders = this.orders;
    } else {
      this.filteredOrders = this.orders.filter(
        order => order.status === parseInt(this.statusFilter)
      );
    }
  }

  updateStatus(orderId: number, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const newStatus = parseInt(select.value);

    this.orderService.updateOrderStatus(orderId, newStatus)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadOrders();
        },
        error: (error) => {
          console.error('Failed to update order status:', error);
          alert('Failed to update order status');
        }
      });
  }

  cancelOrder(orderId: number): void {
    if (confirm('Are you sure you want to cancel this order?')) {
      this.orderService.cancelOrder(orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadOrders();
          },
          error: (error) => {
            console.error('Failed to cancel order:', error);
            alert('Failed to cancel order');
          }
        });
    }
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

  getPaymentLabel(status: PaymentStatus): string {
    const labels: { [key: number]: string } = {
      0: 'Pending',
      1: 'Paid',
      2: 'Failed',
      3: 'Refunded'
    };
    return labels[status] || 'Unknown';
  }

  getPaymentClass(status: PaymentStatus): string {
    const classes: { [key: number]: string } = {
      0: 'payment-pending',
      1: 'payment-paid',
      2: 'payment-failed',
      3: 'payment-refunded'
    };
    return classes[status] || '';
  }

  getPendingCount(): number {
    return this.orders.filter(o => o.status === 0).length;
  }

  getProcessingCount(): number {
    return this.orders.filter(o => o.status === 1).length;
  }

  getShippedCount(): number {
    return this.orders.filter(o => o.status === 2).length;
  }

  getDeliveredCount(): number {
    return this.orders.filter(o => o.status === 3).length;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
