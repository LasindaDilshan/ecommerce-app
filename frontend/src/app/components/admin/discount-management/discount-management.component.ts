import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DiscountCodeService } from '../../../services/discount-code.service';
import { ConfirmationModalService } from '../../../services/confirmation-modal.service';
import { ToastService } from '../../../services/toast.service';
import {
  DiscountCode,
  CreateDiscountCodeRequest,
  DiscountType
} from '../../../models/discount-code.models';

@Component({
  selector: 'app-discount-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-container">
      <div class="header">
        <h1>Discount Code Management</h1>
        <button (click)="showCreateForm = true" class="btn btn-primary">Create New Discount Code</button>
      </div>

      <!-- Create/Edit Form -->
      <div *ngIf="showCreateForm || editingCode" class="discount-form">
        <h2>{{ editingCode ? 'Edit' : 'Create' }} Discount Code</h2>
        <form (ngSubmit)="saveDiscountCode()" #discountForm="ngForm">
          <div class="form-row">
            <div class="form-group">
              <label>Code *</label>
              <input
                type="text"
                class="form-control"
                [(ngModel)]="formData.code"
                name="code"
                [disabled]="!!editingCode"
                required
                maxlength="50"
              />
            </div>
            <div class="form-group">
              <label>Discount Type *</label>
              <select class="form-control" [(ngModel)]="formData.discountType" name="discountType" required>
                <option [value]="1">Percentage</option>
                <option [value]="2">Fixed Amount</option>
                <option [value]="3">Free Shipping</option>
                <option [value]="4">Buy X Get Y</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Value *</label>
              <input
                type="number"
                class="form-control"
                [(ngModel)]="formData.value"
                name="value"
                required
                min="0.01"
                step="0.01"
              />
              <small>{{ getValueHint() }}</small>
            </div>
            <div class="form-group">
              <label>Minimum Purchase</label>
              <input type="number" class="form-control" [(ngModel)]="formData.minimumPurchase" name="minimumPurchase" min="0" step="0.01" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Valid From *</label>
              <input type="datetime-local" class="form-control" [(ngModel)]="formData.validFrom" name="validFrom" required />
            </div>
            <div class="form-group">
              <label>Valid To *</label>
              <input type="datetime-local" class="form-control" [(ngModel)]="formData.validTo" name="validTo" required />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Total Usage Limit</label>
              <input type="number" class="form-control" [(ngModel)]="formData.totalUsageLimit" name="totalUsageLimit" min="1" />
              <small>Leave empty for unlimited</small>
            </div>
            <div class="form-group">
              <label>Per User Limit</label>
              <input type="number" class="form-control" [(ngModel)]="formData.perUserLimit" name="perUserLimit" min="1" />
              <small>Leave empty for unlimited</small>
            </div>
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea class="form-control" [(ngModel)]="formData.description" name="description" rows="3"></textarea>
          </div>

          <div class="form-group">
            <label>
              <input type="checkbox" [(ngModel)]="formData.isActive" name="isActive" />
              Active
            </label>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn btn-success" [disabled]="!discountForm.valid || saving">
              {{ saving ? 'Saving...' : 'Save' }}
            </button>
            <button type="button" class="btn btn-secondary" (click)="cancelEdit()">Cancel</button>
          </div>
        </form>
      </div>

      <!-- Discount Codes List -->
      <div class="discount-list">
        <h2>Existing Discount Codes</h2>
        <div *ngIf="loading" class="loading">Loading discount codes...</div>

        <div *ngIf="!loading && discountCodes.length === 0" class="empty-state">
          No discount codes found. Create your first one!
        </div>

        <div *ngIf="!loading && discountCodes.length > 0" class="table-container">
          <table class="discount-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Type</th>
                <th>Value</th>
                <th>Valid Period</th>
                <th>Usage</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let code of discountCodes" [class.inactive]="!code.isActive">
                <td><strong>{{ code.code }}</strong></td>
                <td>{{ getDiscountTypeDisplay(code.discountType) }}</td>
                <td>{{ formatDiscountValue(code.discountType, code.value) }}</td>
                <td class="small">
                  {{ code.validFrom | date: 'short' }}<br />
                  to {{ code.validTo | date: 'short' }}
                </td>
                <td>
                  {{ code.usedCount }}
                  <span *ngIf="code.totalUsageLimit">/ {{ code.totalUsageLimit }}</span>
                  <span *ngIf="code.isUsageLimitReached" class="badge badge-danger">Limit Reached</span>
                </td>
                <td>
                  <span *ngIf="code.isActive && !code.isExpired" class="badge badge-success">Active</span>
                  <span *ngIf="code.isExpired" class="badge badge-secondary">Expired</span>
                  <span *ngIf="!code.isActive" class="badge badge-danger">Inactive</span>
                </td>
                <td class="actions">
                  <button (click)="editCode(code)" class="btn btn-sm btn-primary">Edit</button>
                  <button (click)="deleteCode(code.id)" class="btn btn-sm btn-danger">Delete</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .admin-container { max-width: 1200px; margin: 0 auto; padding: 20px; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
    .header h1 { margin: 0; }

    .discount-form { background: white; padding: 25px; border-radius: 8px; margin-bottom: 30px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .discount-form h2 { margin-bottom: 20px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; font-weight: 600; margin-bottom: 5px; }
    .form-control { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
    .form-control:focus { outline: none; border-color: #007bff; }
    .form-group small { color: #666; font-size: 12px; }
    .form-actions { display: flex; gap: 10px; margin-top: 20px; }

    .discount-list { background: white; padding: 25px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
    .discount-list h2 { margin-bottom: 20px; }
    .loading, .empty-state { text-align: center; padding: 40px; color: #666; }

    .table-container { overflow-x: auto; }
    .discount-table { width: 100%; border-collapse: collapse; }
    .discount-table th, .discount-table td { padding: 12px; text-align: left; border-bottom: 1px solid #eee; }
    .discount-table th { background: #f8f9fa; font-weight: 600; }
    .discount-table tr.inactive { opacity: 0.6; }
    .discount-table td.small { font-size: 12px; }
    .discount-table td.actions { white-space: nowrap; }

    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-success { background: #d4edda; color: #155724; }
    .badge-danger { background: #f8d7da; color: #721c24; }
    .badge-secondary { background: #e2e3e5; color: #383d41; }

    .btn { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; }
    .btn-primary { background: #007bff; color: white; }
    .btn-primary:hover { background: #0056b3; }
    .btn-success { background: #28a745; color: white; }
    .btn-success:hover { background: #218838; }
    .btn-secondary { background: #6c757d; color: white; }
    .btn-secondary:hover { background: #545b62; }
    .btn-danger { background: #dc3545; color: white; }
    .btn-danger:hover { background: #c82333; }
    .btn-sm { padding: 5px 10px; font-size: 12px; margin-right: 5px; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }
  `]
})
export class DiscountManagementComponent implements OnInit, OnDestroy {
  discountCodes: DiscountCode[] = [];
  loading = false;
  saving = false;
  showCreateForm = false;
  editingCode: DiscountCode | null = null;
  errorMessage = '';
  successMessage = '';

  formData: CreateDiscountCodeRequest = this.getEmptyForm();
  private destroy$ = new Subject<void>();

  constructor(
    private discountCodeService: DiscountCodeService,
    private confirmationService: ConfirmationModalService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadDiscountCodes();
  }

  loadDiscountCodes(): void {
    this.loading = true;
    this.discountCodeService.getAllDiscountCodes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (codes) => {
          this.discountCodes = codes;
          this.loading = false;
        },
        error: (error) => {
          console.error('Failed to load discount codes:', error);
          this.loading = false;
          this.errorMessage = 'Failed to load discount codes';
        }
      });
  }

  saveDiscountCode(): void {
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.editingCode) {
      // Update existing code
      this.discountCodeService.updateDiscountCode(this.editingCode.id, {
        description: this.formData.description,
        isActive: this.formData.isActive
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Discount code updated successfully!');
            this.saving = false;
            this.cancelEdit();
            this.loadDiscountCodes();
          },
          error: (error) => {
            this.saving = false;
            this.toastService.error('Error', error.error?.message || 'Failed to update discount code');
          }
        });
    } else {
      // Create new code
      this.discountCodeService.createDiscountCode(this.formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Discount code created successfully!');
            this.saving = false;
            this.cancelEdit();
            this.loadDiscountCodes();
          },
          error: (error) => {
            this.saving = false;
            this.toastService.error('Error', error.error?.message || 'Failed to create discount code');
          }
        });
    }
  }

  editCode(code: DiscountCode): void {
    this.editingCode = code;
    this.showCreateForm = false;
    this.formData = {
      code: code.code,
      discountType: code.discountType,
      value: code.value,
      minimumPurchase: code.minimumPurchase,
      maximumDiscount: code.maximumDiscount,
      validFrom: new Date(code.validFrom),
      validTo: new Date(code.validTo),
      totalUsageLimit: code.totalUsageLimit,
      perUserLimit: code.perUserLimit,
      buyQuantity: code.buyQuantity,
      getQuantity: code.getQuantity,
      targetProductId: code.targetProductId,
      isActive: code.isActive,
      description: code.description,
      applicableProductIds: code.applicableProductIds,
      applicableCategoryIds: code.applicableCategoryIds
    };
  }

  async deleteCode(id: number): Promise<void> {
    const code = this.discountCodes.find(c => c.id === id);
    const confirmed = await this.confirmationService.confirmDelete(code?.code || 'this discount code');

    if (confirmed) {
      this.discountCodeService.deleteDiscountCode(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Discount code deleted successfully!');
            this.loadDiscountCodes();
          },
          error: (error) => {
            this.toastService.error('Error', error.error?.message || 'Failed to delete discount code');
          }
        });
    }
  }

  cancelEdit(): void {
    this.showCreateForm = false;
    this.editingCode = null;
    this.formData = this.getEmptyForm();
  }

  getEmptyForm(): CreateDiscountCodeRequest {
    const now = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);

    return {
      code: '',
      discountType: DiscountType.Percentage,
      value: 0,
      validFrom: now,
      validTo: nextMonth,
      isActive: true,
      applicableProductIds: [],
      applicableCategoryIds: []
    };
  }

  getDiscountTypeDisplay(type: DiscountType): string {
    return this.discountCodeService.getDiscountTypeDisplay(type);
  }

  formatDiscountValue(type: DiscountType, value: number): string {
    return this.discountCodeService.formatDiscountValue(type, value);
  }

  getValueHint(): string {
    switch (this.formData.discountType) {
      case DiscountType.Percentage:
        return 'Enter percentage (e.g., 20 for 20% off)';
      case DiscountType.FixedAmount:
        return 'Enter dollar amount (e.g., 10 for $10 off)';
      case DiscountType.FreeShipping:
        return 'Value not used for free shipping';
      case DiscountType.BuyXGetY:
        return 'Enter value for Buy X Get Y calculation';
      default:
        return '';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
