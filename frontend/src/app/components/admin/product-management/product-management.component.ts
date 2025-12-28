import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpHeaders } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { ImageService, ImageUploadResult } from '../../../services/image.service';
import { ConfirmationModalService } from '../../../services/confirmation-modal.service';
import { ToastService } from '../../../services/toast.service';
import { environment } from '../../../../environments/environment';
import { Product } from '../../../models/product.models';
import { Category } from '../../../models/category.models';
import { StarRatingComponent } from '../../shared/star-rating/star-rating.component';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule, StarRatingComponent],
  template: `
    <div class="container">
      <div class="header-section">
        <h1>Product Management</h1>
        <button (click)="showAddForm = !showAddForm" class="btn btn-primary">
          {{ showAddForm ? 'Cancel' : 'Add Product' }}
        </button>
      </div>

      <!-- Add/Edit Product Form -->
      <div class="product-form" *ngIf="showAddForm || editingProduct">
        <h2>{{ editingProduct ? 'Edit Product' : 'Add New Product' }}</h2>
        <form (ngSubmit)="saveProduct()" #productForm="ngForm">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Product Name *</label>
              <input type="text" class="form-control" [(ngModel)]="currentProduct.name" name="name" required />
            </div>

            <div class="form-group">
              <label class="form-label">SKU *</label>
              <input type="text" class="form-control" [(ngModel)]="currentProduct.sku" name="sku" required />
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Description</label>
            <textarea class="form-control" [(ngModel)]="currentProduct.description" name="description" rows="3"></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Price *</label>
              <input type="number" class="form-control" [(ngModel)]="currentProduct.price" name="price" step="0.01" required />
            </div>

            <div class="form-group">
              <label class="form-label">Discount Price</label>
              <input type="number" class="form-control" [(ngModel)]="currentProduct.discountPrice" name="discountPrice" step="0.01" />
            </div>

            <div class="form-group">
              <label class="form-label">Stock Quantity *</label>
              <input type="number" class="form-control" [(ngModel)]="currentProduct.stockQuantity" name="stock" required />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Category *</label>
              <select class="form-control" [(ngModel)]="currentProduct.categoryId" name="category" required>
                <option [value]="null">Select Category</option>
                <option *ngFor="let cat of categories" [value]="cat.id">{{ cat.name }}</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Product Image</label>
              <div class="image-upload-container">
                <input type="file"
                  id="imageFile"
                  (change)="onFileSelected($event)"
                  accept="image/jpeg,image/png,image/gif,image/bmp,image/webp,image/tiff"
                  class="file-input" />
                <label for="imageFile" class="file-label">
                  <span class="file-button">Choose File</span>
                  <span class="file-name">{{ selectedFile ? selectedFile.name : 'No file selected' }}</span>
                </label>
                <div *ngIf="imagePreview" class="image-preview">
                  <img [src]="imagePreview" alt="Preview" />
                  <button type="button" class="btn btn-sm btn-danger remove-image" (click)="removeSelectedImage()">×</button>
                </div>
                <div *ngIf="currentProduct.imageUrl && !imagePreview" class="current-image">
                  <p>Current image:</p>
                  <img [src]="getImageUrl(currentProduct.imageUrl)" alt="Current" />
                </div>
                <small class="help-text">Supported formats: JPG, PNG, GIF, BMP, WebP, TIFF. Max size: 10 MB</small>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label class="form-check-label">
              <input type="checkbox" [(ngModel)]="currentProduct.isFeatured" name="featured" />
              Featured Product
            </label>
          </div>

          <div class="form-group">
            <label class="form-check-label">
              <input type="checkbox" [(ngModel)]="currentProduct.isActive" name="active" />
              Active
            </label>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn btn-success" [disabled]="!productForm.valid || saving || uploading">
              {{ uploading ? 'Uploading...' : (saving ? 'Saving...' : 'Save Product') }}
            </button>
            <button type="button" class="btn btn-secondary" (click)="cancelEdit()" [disabled]="saving || uploading">Cancel</button>
          </div>

          <div class="alert alert-error" *ngIf="errorMessage">{{ errorMessage }}</div>
          <div class="alert alert-success" *ngIf="successMessage">{{ successMessage }}</div>
        </form>
      </div>

      <!-- Products Table -->
      <div class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Image</th>
              <th>Name</th>
              <th>SKU</th>
              <th>Category</th>
              <th>Price</th>
              <th>Stock</th>
              <th>Rating</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let product of products">
              <td>
                <img [src]="getImageUrl(product.imageUrl)" [alt]="product.name" class="product-thumbnail" />
              </td>
              <td>
                <strong>{{ product.name }}</strong>
                <span *ngIf="product.isFeatured" class="badge badge-primary">Featured</span>
              </td>
              <td>{{ product.sku }}</td>
              <td>{{ getCategoryName(product.categoryId) }}</td>
              <td>
                <span *ngIf="product.discountPrice" class="price-discount">\${{ product.discountPrice }}</span>
                <span [class.price-original]="product.discountPrice">\${{ product.price }}</span>
              </td>
              <td>
                <span [class.low-stock]="product.stockQuantity < 10">{{ product.stockQuantity }}</span>
              </td>
              <td>
                <app-star-rating
                  [rating]="product.rating"
                  [reviewCount]="product.reviewCount"
                  [size]="'small'"
                  [showCount]="false">
                </app-star-rating>
              </td>
              <td>
                <span [class.badge-success]="product.isActive" [class.badge-danger]="!product.isActive" class="badge">
                  {{ product.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>
                <div class="actions-cell">
                  <button (click)="editProduct(product)" class="btn btn-sm btn-primary">Edit</button>
                  <button (click)="deleteProduct(product.id)" class="btn btn-sm btn-danger">Delete</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="no-data" *ngIf="products.length === 0 && !loading">
          No products found. Add your first product!
        </div>

        <div class="loading" *ngIf="loading">Loading products...</div>
      </div>
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

    .product-form {
      background: var(--bg-card);
      padding: 30px;
      border-radius: 8px;
      margin-bottom: 30px;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
    }

    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
    }

    .form-actions {
      display: flex;
      gap: 10px;
      margin-top: 20px;
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
      background: var(--bg-card);
      color: var(--text-primary);
    }

    .data-table th {
      background: var(--bg-secondary);
      color: var(--text-primary);
      padding: 15px 10px;
      text-align: left;
      font-weight: 600;
      border-bottom: 2px solid var(--border-color);
    }

    .data-table th:nth-child(8),
    .data-table th:nth-child(9) {
      text-align: center;
    }

    .data-table td {
      padding: 12px 10px;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
      vertical-align: middle;
    }

    .data-table td:nth-child(8),
    .data-table td:nth-child(9) {
      text-align: center;
    }

    .data-table tbody tr:hover {
      background: var(--bg-hover);
    }

    .product-thumbnail {
      width: 60px;
      height: 60px;
      object-fit: cover;
      border-radius: 4px;
      border: 1px solid var(--border-color);
    }

    .actions-cell {
      display: flex;
      gap: 8px;
      justify-content: center;
      align-items: center;
      white-space: nowrap;
    }

    .data-table td:last-child {
      min-width: 140px;
      text-align: center;
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
    }

    .badge-danger {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
    }

    .badge-primary {
      background: var(--primary-bg);
      color: var(--primary);
      margin-left: 8px;
    }

    .price-discount {
      color: var(--success);
      font-weight: 600;
      margin-right: 8px;
    }

    .price-original {
      text-decoration: line-through;
      color: var(--text-tertiary);
      font-size: 14px;
    }

    .low-stock {
      color: var(--danger);
      font-weight: 600;
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

    .form-check-label {
      display: flex;
      align-items: center;
      gap: 8px;
      color: var(--text-primary);
      cursor: pointer;
    }

    .form-check-label input[type="checkbox"] {
      width: 18px;
      height: 18px;
      cursor: pointer;
    }

    .image-upload-container {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .file-input {
      display: none;
    }

    .file-label {
      display: flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;
    }

    .file-button {
      background: var(--primary);
      color: white;
      padding: 8px 16px;
      border-radius: 4px;
      font-size: 14px;
      transition: background 0.2s;
    }

    .file-button:hover {
      background: var(--primary-dark);
    }

    .file-name {
      color: var(--text-secondary);
      font-size: 14px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      max-width: 200px;
    }

    .image-preview {
      position: relative;
      display: inline-block;
    }

    .image-preview img {
      max-width: 200px;
      max-height: 150px;
      border-radius: 4px;
      border: 1px solid var(--border-color);
    }

    .remove-image {
      position: absolute;
      top: -8px;
      right: -8px;
      width: 24px;
      height: 24px;
      border-radius: 50%;
      padding: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
      line-height: 1;
    }

    .current-image {
      margin-top: 5px;
    }

    .current-image p {
      margin: 0 0 5px;
      font-size: 12px;
      color: var(--text-secondary);
    }

    .current-image img {
      max-width: 150px;
      max-height: 100px;
      border-radius: 4px;
      border: 1px solid var(--border-color);
    }

    .help-text {
      color: var(--text-tertiary);
      font-size: 12px;
    }

    @media (max-width: 768px) {
      .table-container {
        overflow-x: auto;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .actions-cell {
        flex-direction: column;
        gap: 4px;
      }

      .data-table td:last-child {
        min-width: 100px;
      }

      .btn-sm {
        padding: 4px 8px;
        font-size: 12px;
      }
    }
  `]
})
export class ProductManagementComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  categories: Category[] = [];
  showAddForm = false;
  editingProduct = false;
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  currentProduct: any = this.getEmptyProduct();
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  // Image upload properties
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading = false;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private imageService: ImageService,
    private confirmationService: ConfirmationModalService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.loading = true;
    // Add cache-bypass headers to ensure fresh data after updates
    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache'
    });
    this.productService.getProducts({ pageNumber: 1, pageSize: 100 }, headers)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.products = result.items;
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load products';
          this.loading = false;
        }
      });
  }

  loadCategories(): void {
    this.categoryService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          this.categories = categories;
        },
        error: (error) => {
          console.error('Failed to load categories', error);
        }
      });
  }

  getEmptyProduct(): any {
    return {
      name: '',
      description: '',
      price: 0,
      discountPrice: null,
      stockQuantity: 0,
      sku: '',
      imageUrl: '',
      categoryId: null,
      isFeatured: false,
      isActive: true
    };
  }

  editProduct(product: Product): void {
    this.editingProduct = true;
    this.showAddForm = false;
    this.currentProduct = { ...product };
    this.errorMessage = '';
    this.successMessage = '';
    this.removeSelectedImage();
  }

  saveProduct(): void {
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    // If there's a selected file, upload it first
    if (this.selectedFile) {
      this.uploading = true;
      this.imageService.uploadImage(this.selectedFile)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result) => {
            this.uploading = false;
            // Use the medium size URL for the product
            this.currentProduct.imageUrl = result.mediumUrl;
            this.saveProductData();
          },
          error: (error) => {
            this.uploading = false;
            this.saving = false;
            this.errorMessage = error.error?.message || 'Failed to upload image';
          }
        });
    } else {
      this.saveProductData();
    }
  }

  private saveProductData(): void {
    const productData = {
      ...this.currentProduct,
      price: parseFloat(this.currentProduct.price),
      discountPrice: this.currentProduct.discountPrice ? parseFloat(this.currentProduct.discountPrice) : null,
      stockQuantity: parseInt(this.currentProduct.stockQuantity)
    };

    if (this.editingProduct && this.currentProduct.id) {
      // Update existing product
      this.productService.updateProduct(this.currentProduct.id, productData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.successMessage = 'Product updated successfully!';
            this.saving = false;
            this.loadProducts();
            this.timeoutIds.push(setTimeout(() => this.cancelEdit(), 2000));
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Failed to update product';
            this.saving = false;
          }
        });
    } else {
      // Create new product
      this.productService.createProduct(productData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.successMessage = 'Product created successfully!';
            this.saving = false;
            this.loadProducts();
            this.timeoutIds.push(setTimeout(() => this.cancelEdit(), 2000));
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Failed to create product';
            this.saving = false;
          }
        });
    }
  }

  async deleteProduct(id: number): Promise<void> {
    const product = this.products.find(p => p.id === id);
    const confirmed = await this.confirmationService.confirmDelete(product?.name || 'this product');

    if (confirmed) {
      this.productService.deleteProduct(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toastService.success('Product Deleted', 'Product has been deleted successfully');
            this.loadProducts();
          },
          error: (error) => {
            this.toastService.error('Delete Failed', error.error?.message || 'Failed to delete product');
          }
        });
    }
  }

  cancelEdit(): void {
    this.editingProduct = false;
    this.showAddForm = false;
    this.currentProduct = this.getEmptyProduct();
    this.errorMessage = '';
    this.successMessage = '';
    this.removeSelectedImage();
  }

  getCategoryName(categoryId: number): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown';
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // Validate file size (10 MB)
      if (file.size > 10 * 1024 * 1024) {
        this.errorMessage = 'File size exceeds 10 MB limit';
        return;
      }

      // Validate file type
      const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp', 'image/webp', 'image/tiff'];
      if (!validTypes.includes(file.type)) {
        this.errorMessage = 'Invalid file type. Supported: JPG, PNG, GIF, BMP, WebP, TIFF';
        return;
      }

      this.selectedFile = file;
      this.errorMessage = '';

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeSelectedImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
    // Reset file input
    const fileInput = document.getElementById('imageFile') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/60x60/CCCCCC/FFFFFF?text=No+Image';
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
