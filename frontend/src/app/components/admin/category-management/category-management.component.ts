import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CategoryService } from '../../../services/category.service';
import { ConfirmationModalService } from '../../../services/confirmation-modal.service';
import { ToastService } from '../../../services/toast.service';
import { Category } from '../../../models/category.models';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <div class="header-section">
        <h1>Category Management</h1>
        <button (click)="showAddForm = !showAddForm" class="btn btn-primary">
          {{ showAddForm ? 'Cancel' : 'Add Category' }}
        </button>
      </div>

      <!-- Add/Edit Form -->
      <div class="category-form" *ngIf="showAddForm || editingCategory">
        <h2>{{ editingCategory ? 'Edit Category' : 'Add New Category' }}</h2>
        <form (ngSubmit)="saveCategory()" #categoryForm="ngForm">
          <div class="form-group">
            <label class="form-label">Category Name *</label>
            <input type="text" class="form-control" [(ngModel)]="currentCategory.name" name="name" required />
          </div>

          <div class="form-group">
            <label class="form-label">Description</label>
            <textarea class="form-control" [(ngModel)]="currentCategory.description" name="description" rows="3"></textarea>
          </div>

          <div class="form-group">
            <label class="form-check-label">
              <input type="checkbox" [(ngModel)]="currentCategory.isActive" name="active" />
              Active
            </label>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn btn-success" [disabled]="!categoryForm.valid || saving">
              {{ saving ? 'Saving...' : 'Save Category' }}
            </button>
            <button type="button" class="btn btn-secondary" (click)="cancelEdit()">Cancel</button>
          </div>

          <div class="alert alert-error" *ngIf="errorMessage">{{ errorMessage }}</div>
          <div class="alert alert-success" *ngIf="successMessage">{{ successMessage }}</div>
        </form>
      </div>

      <!-- Categories Grid -->
      <div class="categories-grid">
        <div *ngFor="let category of categories" class="category-card">
          <div class="category-header">
            <h3>{{ category.name }}</h3>
            <span [class.badge-success]="category.isActive" [class.badge-danger]="!category.isActive" class="badge">
              {{ category.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
          <p class="category-description">{{ category.description || 'No description' }}</p>
          <div class="category-stats">
            <span class="stat-badge">
              <strong>{{ category.productCount || 0 }}</strong> Products
            </span>
          </div>
          <div class="category-actions">
            <button (click)="editCategory(category)" class="btn btn-sm btn-primary">Edit</button>
            <button (click)="deleteCategory(category.id)" class="btn btn-sm btn-danger" [disabled]="category.productCount > 0">
              Delete
            </button>
          </div>
        </div>
      </div>

      <div class="no-data" *ngIf="categories.length === 0 && !loading">
        No categories found. Add your first category!
      </div>

      <div class="loading" *ngIf="loading">Loading categories...</div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
    }

    .header-section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }

    h1 {
      color: var(--text-primary);
      margin: 0;
    }

    h2 {
      color: var(--text-primary);
      margin-bottom: 20px;
    }

    .category-form {
      background: var(--bg-card);
      padding: 30px;
      border-radius: 8px;
      margin-bottom: 30px;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
    }

    .form-actions {
      display: flex;
      gap: 10px;
      margin-top: 20px;
    }

    .form-check-label {
      display: flex;
      align-items: center;
      gap: 8px;
      color: var(--text-primary);
      cursor: pointer;
    }

    .categories-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }

    .category-card {
      background: var(--bg-card);
      padding: 20px;
      border-radius: 8px;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .category-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-lg);
    }

    .category-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
    }

    .category-header h3 {
      color: var(--text-primary);
      margin: 0;
      font-size: 18px;
    }

    .category-description {
      color: var(--text-secondary);
      margin: 10px 0;
      font-size: 14px;
      min-height: 40px;
    }

    .category-stats {
      margin: 15px 0;
    }

    .stat-badge {
      display: inline-block;
      padding: 6px 12px;
      background: var(--bg-secondary);
      color: var(--text-primary);
      border-radius: 4px;
      font-size: 14px;
      border: 1px solid var(--border-color);
    }

    .stat-badge strong {
      color: var(--primary);
    }

    .category-actions {
      display: flex;
      gap: 8px;
      margin-top: 15px;
    }

    .btn-sm {
      padding: 6px 12px;
      font-size: 14px;
    }

    .badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }

    .badge-success {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
      border: 1px solid var(--success);
    }

    .badge-danger {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
      border: 1px solid var(--danger);
    }

    .no-data {
      padding: 40px;
      text-align: center;
      color: var(--text-secondary);
    }

    .loading {
      padding: 40px;
      text-align: center;
      color: var(--text-secondary);
    }

    @media (max-width: 768px) {
      .categories-grid {
        grid-template-columns: 1fr;
      }

      .header-section {
        flex-direction: column;
        gap: 15px;
        align-items: flex-start;
      }
    }
  `]
})
export class CategoryManagementComponent implements OnInit, OnDestroy {
  categories: Category[] = [];
  showAddForm = false;
  editingCategory = false;
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  currentCategory: any = this.getEmptyCategory();
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  constructor(
    private categoryService: CategoryService,
    private confirmationService: ConfirmationModalService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          this.categories = categories;
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load categories';
          this.loading = false;
        }
      });
  }

  getEmptyCategory(): any {
    return {
      name: '',
      description: '',
      isActive: true
    };
  }

  editCategory(category: Category): void {
    this.editingCategory = true;
    this.showAddForm = false;
    this.currentCategory = { ...category };
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveCategory(): void {
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.editingCategory && this.currentCategory.id) {
      this.categoryService.updateCategory(this.currentCategory.id, this.currentCategory)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Category updated successfully!');
            this.saving = false;
            this.loadCategories();
            this.cancelEdit();
          },
          error: (error) => {
            this.toastService.error('Error', error.error?.message || 'Failed to update category');
            this.saving = false;
          }
        });
    } else {
      this.categoryService.createCategory(this.currentCategory)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Category created successfully!');
            this.saving = false;
            this.loadCategories();
            this.cancelEdit();
          },
          error: (error) => {
            this.toastService.error('Error', error.error?.message || 'Failed to create category');
            this.saving = false;
          }
        });
    }
  }

  async deleteCategory(id: number): Promise<void> {
    const category = this.categories.find(c => c.id === id);
    const confirmed = await this.confirmationService.confirmDelete(category?.name || 'this category');

    if (confirmed) {
      this.categoryService.deleteCategory(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Success', 'Category deleted successfully!');
            this.loadCategories();
          },
          error: (error) => {
            this.toastService.error('Error', error.error?.message || 'Failed to delete category');
          }
        });
    }
  }

  cancelEdit(): void {
    this.editingCategory = false;
    this.showAddForm = false;
    this.currentCategory = this.getEmptyCategory();
    this.errorMessage = '';
    this.successMessage = '';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timeoutIds.forEach(id => clearTimeout(id));
  }
}
