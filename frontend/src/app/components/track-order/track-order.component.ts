import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../../services/order.service';
import { GuestOrderResponse } from '../../models/order.models';

@Component({
  selector: 'app-track-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <h1>Track Your Order</h1>

      <div class="track-form" *ngIf="!orderDetails">
        <p class="info">Enter your order number and email to track your order</p>

        <form (ngSubmit)="trackOrder()" #trackForm="ngForm">
          <div class="form-group">
            <input
              class="form-control"
              placeholder="Order Number *"
              [(ngModel)]="orderNumber"
              name="orderNumber"
              required
            />
          </div>
          <div class="form-group">
            <input
              type="email"
              class="form-control"
              placeholder="Email *"
              [(ngModel)]="email"
              name="email"
              required
            />
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="!trackForm.valid || loading">
            {{ loading ? 'Searching...' : 'Track Order' }}
          </button>
        </form>

        <p class="error-message" *ngIf="errorMessage">{{ errorMessage }}</p>
      </div>

      <div class="order-details" *ngIf="orderDetails">
        <div class="order-header">
          <h2>Order #{{ orderDetails.orderNumber }}</h2>
          <span class="status-badge" [class]="'status-' + orderDetails.status.toLowerCase()">
            {{ orderDetails.status }}
          </span>
        </div>

        <div class="order-info">
          <div class="info-row">
            <span class="label">Customer:</span>
            <span class="value">{{ orderDetails.firstName }} {{ orderDetails.lastName }}</span>
          </div>
          <div class="info-row">
            <span class="label">Email:</span>
            <span class="value">{{ orderDetails.email }}</span>
          </div>
          <div class="info-row">
            <span class="label">Order Date:</span>
            <span class="value">{{ orderDetails.orderDate | date: 'medium' }}</span>
          </div>
          <div class="info-row">
            <span class="label">Total Amount:</span>
            <span class="value total">\${{ orderDetails.totalAmount.toFixed(2) }}</span>
          </div>
        </div>

        <button class="btn btn-secondary" (click)="resetTracking()">Track Another Order</button>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 600px;
      margin: 40px auto;
      padding: 20px;
    }

    h1 {
      text-align: center;
      margin-bottom: 30px;
      color: #333;
    }

    .track-form {
      background: white;
      padding: 30px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .info {
      text-align: center;
      color: #666;
      margin-bottom: 25px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    .form-control {
      width: 100%;
      padding: 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 16px;
    }

    .form-control:focus {
      outline: none;
      border-color: #007bff;
    }

    .btn {
      width: 100%;
      padding: 12px;
      border: none;
      border-radius: 4px;
      font-size: 16px;
      cursor: pointer;
      transition: background 0.3s;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn-primary:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
      margin-top: 20px;
    }

    .btn-secondary:hover {
      background: #545b62;
    }

    .error-message {
      margin-top: 15px;
      padding: 12px;
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
      text-align: center;
    }

    .order-details {
      background: white;
      padding: 30px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .order-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 25px;
      padding-bottom: 20px;
      border-bottom: 2px solid #eee;
    }

    .order-header h2 {
      margin: 0;
      color: #333;
    }

    .status-badge {
      padding: 8px 16px;
      border-radius: 20px;
      font-size: 14px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .status-pending {
      background: #ffc107;
      color: #856404;
    }

    .status-processing {
      background: #17a2b8;
      color: white;
    }

    .status-shipped {
      background: #007bff;
      color: white;
    }

    .status-delivered {
      background: #28a745;
      color: white;
    }

    .status-cancelled {
      background: #dc3545;
      color: white;
    }

    .order-info {
      margin-bottom: 20px;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid #eee;
    }

    .info-row:last-child {
      border-bottom: none;
    }

    .label {
      font-weight: 600;
      color: #666;
    }

    .value {
      color: #333;
    }

    .value.total {
      font-size: 20px;
      font-weight: bold;
      color: #007bff;
    }
  `]
})
export class TrackOrderComponent implements OnInit, OnDestroy {
  orderNumber = '';
  email = '';
  loading = false;
  errorMessage = '';
  orderDetails: GuestOrderResponse | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if order info was passed via query params (from checkout)
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['orderNumber'] && params['email']) {
          this.orderNumber = params['orderNumber'];
          this.email = params['email'];
          this.trackOrder();
        }
      });
  }

  trackOrder(): void {
    this.loading = true;
    this.errorMessage = '';

    this.orderService.trackGuestOrder({
      orderNumber: this.orderNumber,
      email: this.email
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (order) => {
          this.orderDetails = order;
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Order not found. Please check your order number and email.';
        }
      });
  }

  resetTracking(): void {
    this.orderDetails = null;
    this.orderNumber = '';
    this.email = '';
    this.errorMessage = '';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
