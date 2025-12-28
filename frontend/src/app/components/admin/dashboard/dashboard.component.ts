import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DashboardService } from '../../../services/dashboard.service';
import { DashboardStats } from '../../../models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h1>Admin Dashboard</h1>

      <div class="stats-grid" *ngIf="stats">
        <div class="stat-card">
          <h3>Total Revenue</h3>
          <p>\${{ stats.totalRevenue }}</p>
        </div>
        <div class="stat-card">
          <h3>Total Orders</h3>
          <p>{{ stats.totalOrders }}</p>
        </div>
        <div class="stat-card">
          <h3>Total Customers</h3>
          <p>{{ stats.totalCustomers }}</p>
        </div>
        <div class="stat-card">
          <h3>Total Products</h3>
          <p>{{ stats.totalProducts }}</p>
        </div>
      </div>

      <div class="admin-links">
        <a routerLink="/admin/products" class="btn btn-primary">Manage Products</a>
        <a routerLink="/admin/categories" class="btn btn-primary">Manage Categories</a>
        <a routerLink="/admin/orders" class="btn btn-primary">Manage Orders</a>
        <a routerLink="/admin/users" class="btn btn-primary">Manage Users</a>
        <a routerLink="/admin/inventory" class="btn btn-primary">Manage Inventory</a>
        <a routerLink="/admin/discounts" class="btn btn-primary">Manage Discounts</a>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
    }

    h1 {
      color: var(--text-primary);
      margin-bottom: 30px;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 20px;
      margin-bottom: 30px;
    }

    .stat-card {
      background: var(--bg-card);
      padding: 30px;
      border-radius: 8px;
      text-align: center;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-lg);
    }

    .stat-card h3 {
      color: var(--text-secondary);
      font-size: 14px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 1px;
      margin-bottom: 10px;
    }

    .stat-card p {
      color: var(--text-primary);
      font-size: 32px;
      font-weight: 700;
      margin: 0;
    }

    .admin-links {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 15px;
    }

    .admin-links .btn {
      padding: 15px 20px;
      text-align: center;
      font-weight: 600;
    }

    @media (max-width: 768px) {
      .stats-grid {
        grid-template-columns: 1fr;
      }

      .admin-links {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  stats: DashboardStats | null = null;
  private destroy$ = new Subject<void>();

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe(stats => {
        this.stats = stats;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
