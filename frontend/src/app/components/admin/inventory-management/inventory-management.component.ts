import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { InventoryService, StockItemDto, WarehouseDto, StockMovementDto, InventoryReportDto } from '../../../services/inventory.service';

@Component({
  selector: 'app-inventory-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="header-section">
        <h1>Inventory Management</h1>
        <div class="header-actions">
          <button (click)="initializeStock()" [disabled]="initializing" class="btn btn-primary">
            {{ initializing ? 'Initializing...' : 'Sync from Products' }}
          </button>
          <select [(ngModel)]="selectedWarehouseId" (change)="loadInventory()" class="warehouse-select">
            <option [value]="null">All Warehouses</option>
            <option *ngFor="let warehouse of warehouses" [value]="warehouse.id">
              {{ warehouse.name }}
            </option>
          </select>
        </div>
      </div>

      <!-- Success/Error Messages -->
      <div *ngIf="successMessage" class="alert alert-success">{{ successMessage }}</div>
      <div *ngIf="errorMessage" class="alert alert-error">{{ errorMessage }}</div>

      <!-- Summary Cards -->
      <div class="summary-cards" *ngIf="report">
        <div class="summary-card">
          <div class="card-icon">📦</div>
          <div class="card-content">
            <h3>{{ report.totalProducts }}</h3>
            <p>Total Products</p>
          </div>
        </div>
        <div class="summary-card">
          <div class="card-icon">💰</div>
          <div class="card-content">
            <h3>\${{ report.totalStockValue.toFixed(2) }}</h3>
            <p>Total Stock Value</p>
          </div>
        </div>
        <div class="summary-card warning" *ngIf="report.lowStockItems > 0">
          <div class="card-icon">⚠️</div>
          <div class="card-content">
            <h3>{{ report.lowStockItems }}</h3>
            <p>Low Stock Items</p>
          </div>
        </div>
        <div class="summary-card danger" *ngIf="report.outOfStockItems > 0">
          <div class="card-icon">🚫</div>
          <div class="card-content">
            <h3>{{ report.outOfStockItems }}</h3>
            <p>Out of Stock</p>
          </div>
        </div>
      </div>

      <!-- Low Stock Alerts -->
      <div class="low-stock-section" *ngIf="lowStockItems.length > 0">
        <h2>⚠️ Low Stock Alerts</h2>
        <div class="low-stock-grid">
          <div *ngFor="let item of lowStockItems" class="low-stock-card">
            <div class="product-info">
              <strong>{{ item.productName }}</strong>
              <span class="sku">SKU: {{ item.productSku }}</span>
            </div>
            <div class="stock-info">
              <span class="quantity" [class.critical]="item.quantityOnHand === 0">
                {{ item.quantityOnHand }} in stock
              </span>
              <span class="reorder-point">Reorder at: {{ item.reorderPoint }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Stock Table -->
      <div class="table-section">
        <h2>Stock Levels</h2>
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Warehouse</th>
                <th>On Hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Reorder Point</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of stockItems" [class.low-stock-row]="item.quantityOnHand <= item.reorderPoint">
                <td>{{ item.productName }}</td>
                <td>{{ item.productSku }}</td>
                <td>{{ item.warehouseName }}</td>
                <td>{{ item.quantityOnHand }}</td>
                <td>{{ item.quantityReserved }}</td>
                <td>{{ item.quantityAvailable }}</td>
                <td>{{ item.reorderPoint }}</td>
                <td>
                  <span class="status-badge" [class]="getStockStatusClass(item)">
                    {{ getStockStatus(item) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="no-data" *ngIf="stockItems.length === 0 && !loading">
            No stock items found.
          </div>

          <div class="loading" *ngIf="loading">Loading inventory...</div>
        </div>
      </div>

      <!-- Recent Stock Movements -->
      <div class="movements-section" *ngIf="movements.length > 0">
        <h2>Recent Stock Movements</h2>
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Product</th>
                <th>Type</th>
                <th>Quantity</th>
                <th>Reference</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let movement of movements">
                <td>{{ movement.createdAt | date:'short' }}</td>
                <td>{{ movement.productName }}</td>
                <td>
                  <span class="movement-type" [class]="getMovementTypeClass(movement.type)">
                    {{ movement.type }}
                  </span>
                </td>
                <td [class.positive]="isPositiveMovement(movement.type)" [class.negative]="!isPositiveMovement(movement.type)">
                  {{ isPositiveMovement(movement.type) ? '+' : '-' }}{{ movement.quantity }}
                </td>
                <td>{{ movement.reference || '-' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      padding-top: 100px;
    }

    .header-section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }

    .header-actions {
      display: flex;
      gap: 15px;
      align-items: center;
    }

    .btn-primary {
      padding: 10px 20px;
      background: var(--primary);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-primary:hover:not(:disabled) {
      background: var(--primary-dark);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .alert {
      padding: 12px 16px;
      border-radius: 8px;
      margin-bottom: 20px;
    }

    .alert-success {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
      border: 1px solid var(--success);
    }

    .alert-error {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
      border: 1px solid var(--danger);
    }

    h1 {
      color: var(--text-primary);
      margin: 0;
    }

    h2 {
      color: var(--text-primary);
      margin-bottom: 20px;
      font-size: 1.25rem;
    }

    .warehouse-select {
      padding: 10px 16px;
      border-radius: 8px;
      border: 1px solid var(--border-color);
      background: var(--bg-card);
      color: var(--text-primary);
      font-size: 14px;
      min-width: 200px;
    }

    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      margin-bottom: 30px;
    }

    .summary-card {
      background: var(--bg-card);
      padding: 20px;
      border-radius: 12px;
      border: 1px solid var(--border-color);
      display: flex;
      align-items: center;
      gap: 15px;
      box-shadow: var(--shadow-sm);
    }

    .summary-card.warning {
      border-color: var(--warning);
      background: rgba(245, 158, 11, 0.05);
    }

    .summary-card.danger {
      border-color: var(--danger);
      background: rgba(239, 68, 68, 0.05);
    }

    .card-icon {
      font-size: 2rem;
    }

    .card-content h3 {
      margin: 0;
      font-size: 1.5rem;
      color: var(--text-primary);
    }

    .card-content p {
      margin: 4px 0 0;
      color: var(--text-secondary);
      font-size: 14px;
    }

    .low-stock-section {
      margin-bottom: 30px;
    }

    .low-stock-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 15px;
    }

    .low-stock-card {
      background: rgba(245, 158, 11, 0.1);
      border: 1px solid var(--warning);
      border-radius: 8px;
      padding: 15px;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .product-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .product-info strong {
      color: var(--text-primary);
    }

    .product-info .sku {
      color: var(--text-secondary);
      font-size: 12px;
    }

    .stock-info {
      text-align: right;
    }

    .stock-info .quantity {
      display: block;
      font-weight: 600;
      color: var(--warning);
    }

    .stock-info .quantity.critical {
      color: var(--danger);
    }

    .stock-info .reorder-point {
      font-size: 12px;
      color: var(--text-secondary);
    }

    .table-section, .movements-section {
      margin-bottom: 30px;
    }

    .table-container {
      background: var(--bg-card);
      border-radius: 8px;
      overflow: hidden;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
    }

    .data-table {
      width: 100%;
      border-collapse: collapse;
    }

    .data-table th {
      background: var(--bg-secondary);
      color: var(--text-primary);
      padding: 12px 10px;
      text-align: left;
      font-weight: 600;
      border-bottom: 2px solid var(--border-color);
    }

    .data-table td {
      padding: 12px 10px;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }

    .data-table tbody tr:hover {
      background: var(--bg-hover);
    }

    .low-stock-row {
      background: rgba(245, 158, 11, 0.05);
    }

    .status-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }

    .status-in-stock {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
    }

    .status-low-stock {
      background: rgba(245, 158, 11, 0.1);
      color: var(--warning);
    }

    .status-out-of-stock {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
    }

    .movement-type {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }

    .movement-receipt {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
    }

    .movement-sale {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
    }

    .movement-return {
      background: rgba(59, 130, 246, 0.1);
      color: var(--info);
    }

    .movement-adjustment {
      background: rgba(245, 158, 11, 0.1);
      color: var(--warning);
    }

    .positive {
      color: var(--success);
      font-weight: 600;
    }

    .negative {
      color: var(--danger);
      font-weight: 600;
    }

    .no-data, .loading {
      padding: 40px;
      text-align: center;
      color: var(--text-secondary);
    }

    @media (max-width: 768px) {
      .header-section {
        flex-direction: column;
        gap: 15px;
        align-items: flex-start;
      }

      .warehouse-select {
        width: 100%;
      }

      .table-container {
        overflow-x: auto;
      }
    }
  `]
})
export class InventoryManagementComponent implements OnInit, OnDestroy {
  warehouses: WarehouseDto[] = [];
  stockItems: StockItemDto[] = [];
  lowStockItems: StockItemDto[] = [];
  movements: StockMovementDto[] = [];
  report: InventoryReportDto | null = null;
  selectedWarehouseId: number | null = null;
  loading = false;
  initializing = false;
  successMessage = '';
  errorMessage = '';
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  constructor(private inventoryService: InventoryService) {}

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadInventory();
  }

  loadWarehouses(): void {
    this.inventoryService.getWarehouses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (warehouses) => {
          this.warehouses = warehouses;
        },
        error: (error) => {
          console.error('Error loading warehouses:', error);
        }
      });
  }

  loadInventory(): void {
    this.loading = true;

    // Load inventory report
    this.inventoryService.getInventoryReport(this.selectedWarehouseId || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.report = report;
          this.stockItems = report.stockItems || [];
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading inventory report:', error);
          this.loading = false;
        }
      });

    // Load low stock items
    this.inventoryService.getLowStockItems(this.selectedWarehouseId || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          this.lowStockItems = items;
        },
        error: (error) => {
          console.error('Error loading low stock items:', error);
        }
      });

    // Load recent movements
    this.inventoryService.getStockMovements(undefined, this.selectedWarehouseId || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (movements) => {
          this.movements = movements.slice(0, 10); // Show last 10
        },
        error: (error) => {
          console.error('Error loading movements:', error);
        }
      });
  }

  initializeStock(): void {
    this.initializing = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.inventoryService.initializeFromProducts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.successMessage = `${result.message} Created: ${result.created}, Updated: ${result.updated}`;
          this.initializing = false;
          this.loadInventory();

          const timeoutId = setTimeout(() => {
            this.successMessage = '';
          }, 5000);
          this.timeoutIds.push(timeoutId);
        },
        error: (error) => {
          console.error('Error initializing stock:', error);
          this.errorMessage = error.error?.message || 'Failed to initialize stock from products';
          this.initializing = false;

          const timeoutId = setTimeout(() => {
            this.errorMessage = '';
          }, 5000);
          this.timeoutIds.push(timeoutId);
        }
      });
  }

  getStockStatus(item: StockItemDto): string {
    if (item.quantityOnHand === 0) return 'Out of Stock';
    if (item.quantityOnHand <= item.reorderPoint) return 'Low Stock';
    return 'In Stock';
  }

  getStockStatusClass(item: StockItemDto): string {
    if (item.quantityOnHand === 0) return 'status-out-of-stock';
    if (item.quantityOnHand <= item.reorderPoint) return 'status-low-stock';
    return 'status-in-stock';
  }

  getMovementTypeClass(type: string): string {
    const typeMap: { [key: string]: string } = {
      'Receipt': 'movement-receipt',
      'Sale': 'movement-sale',
      'Return': 'movement-return',
      'Adjustment': 'movement-adjustment'
    };
    return typeMap[type] || '';
  }

  isPositiveMovement(type: string): boolean {
    return ['Receipt', 'Return'].includes(type);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timeoutIds.forEach(id => clearTimeout(id));
  }
}
