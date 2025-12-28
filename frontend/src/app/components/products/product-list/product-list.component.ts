import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpHeaders } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, take } from 'rxjs/operators';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { WishlistService } from '../../../services/wishlist.service';
import { AuthService } from '../../../services/auth.service';
import { ComparisonService } from '../../../services/comparison.service';
import { ToastService } from '../../../services/toast.service';
import { Product, ProductQueryParams } from '../../../models/product.models';
import { Category } from '../../../models/category.models';
import { environment } from '../../../../environments/environment';
import { StarRatingComponent } from '../../shared/star-rating/star-rating.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StarRatingComponent],
  template: `
    <div class="container animate-fadeIn">
      <div class="page-header">
        <h1>Products</h1>
        <p class="subtitle">Browse our collection of amazing products</p>
      </div>

      <div class="filters">
        <input
          type="text"
          class="form-control"
          placeholder="Search products..."
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearchChange()"
        />

        <select class="form-control" [(ngModel)]="selectedCategory" (ngModelChange)="onFilterChange()">
          <option [ngValue]="undefined">All Categories</option>
          <option *ngFor="let cat of categories" [ngValue]="cat.id">{{ cat.name }}</option>
        </select>
      </div>

      <div class="product-grid" *ngIf="!loading">
        <div class="product-card" *ngFor="let product of products">
          <div class="product-image-container">
            <img [src]="getImageUrl(product.imageUrl)" [alt]="product.name" />
            <button
              *ngIf="isLoggedIn"
              (click)="toggleWishlist(product)"
              class="wishlist-btn"
              [class.in-wishlist]="isInWishlist(product.id)"
              title="Add to wishlist">
              ❤
            </button>
            <button
              (click)="toggleComparison(product)"
              class="compare-btn"
              [class.in-comparison]="isInComparison(product.id)"
              [disabled]="!isInComparison(product.id) && getComparisonCount() >= 4"
              [title]="getCompareButtonTitle(product.id)">
              ⚖️
            </button>
          </div>
          <h3>{{ product.name }}</h3>
          <div class="rating-wrapper">
            <app-star-rating
              [rating]="product.rating"
              [reviewCount]="product.reviewCount"
              [size]="'small'"
              [showValue]="false">
            </app-star-rating>
          </div>
          <p class="price">
            <span *ngIf="product.discountPrice" class="original-price">\${{ product.price }}</span>
            <span class="current-price">\${{ product.discountPrice || product.price }}</span>
          </p>
          <a [routerLink]="['/products', product.id]" class="btn btn-primary">View</a>
        </div>
      </div>

      <div class="pagination" *ngIf="totalPages > 1">
        <button (click)="changePage(currentPage - 1)" [disabled]="currentPage === 1">Previous</button>
        <span>Page {{ currentPage }} of {{ totalPages }}</span>
        <button (click)="changePage(currentPage + 1)" [disabled]="currentPage === totalPages">Next</button>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
      min-height: calc(100vh - 180px);
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

    .filters {
      display: flex;
      gap: 20px;
      margin-bottom: 30px;
    }

    .filters .form-control {
      flex: 1;
      max-width: 400px;
    }

    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 24px;
      margin-bottom: 40px;
    }

    .product-card {
      background: var(--bg-card);
      color: var(--text-primary);
      padding: 20px;
      border-radius: 12px;
      text-align: center;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
      transition: all 0.3s ease;
    }

    .product-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-lg);
    }

    .product-image-container {
      position: relative;
      margin-bottom: 15px;
    }

    .product-card img {
      width: 100%;
      height: 200px;
      object-fit: contain;
      border-radius: 8px;
      background: var(--bg-secondary);
    }

    .product-card h3 {
      color: var(--text-primary);
      font-size: 18px;
      font-weight: 600;
      margin: 10px 0;
      min-height: 48px;
    }

    .rating-wrapper {
      display: flex;
      justify-content: center;
      margin: 8px 0;
      min-height: 24px;
    }

    .wishlist-btn {
      position: absolute;
      top: 10px;
      right: 10px;
      background: var(--bg-card);
      border: 2px solid var(--border-color);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      font-size: 20px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      color: var(--text-tertiary);
    }

    .wishlist-btn:hover {
      transform: scale(1.1);
      border-color: var(--danger);
    }

    .wishlist-btn.in-wishlist {
      background: var(--danger);
      border-color: var(--danger);
      color: white;
    }

    .compare-btn {
      position: absolute;
      top: 10px;
      left: 10px;
      background: var(--bg-card);
      border: 2px solid var(--border-color);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      font-size: 20px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      color: var(--text-tertiary);
    }

    .compare-btn:hover:not(:disabled) {
      transform: scale(1.1);
      border-color: #667eea;
      color: #667eea;
    }

    .compare-btn.in-comparison {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-color: #667eea;
      color: white;
    }

    .compare-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .price {
      margin: 10px 0;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
    }

    .original-price {
      text-decoration: line-through;
      color: var(--text-tertiary);
      font-size: 14px;
    }

    .current-price {
      font-size: 24px;
      font-weight: 700;
      color: var(--primary);
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 20px;
      margin-top: 40px;
      padding: 20px 0;
    }

    .pagination span {
      color: var(--text-primary);
      font-weight: 600;
    }

    .pagination button {
      padding: 10px 20px;
      background: var(--primary);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .pagination button:hover:not(:disabled) {
      background: var(--primary-dark);
      transform: translateY(-2px);
    }

    .pagination button:disabled {
      background: var(--bg-secondary);
      color: var(--text-tertiary);
      cursor: not-allowed;
      opacity: 0.5;
    }

    .animate-fadeIn {
      animation: fadeIn 0.6s ease-out;
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

    @media (max-width: 768px) {
      .container {
        padding: 15px;
      }

      .page-header h1 {
        font-size: 24px;
      }

      .filters {
        flex-direction: column;
      }

      .filters .form-control {
        max-width: 100%;
      }

      .product-grid {
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 16px;
      }
    }
  `]
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  categories: Category[] = [];
  loading = true;
  searchTerm = '';
  selectedCategory: number | undefined = undefined;
  currentPage = 1;
  totalPages = 1;
  wishlistProductIds: Set<number> = new Set();
  isLoggedIn = false;
  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private comparisonService: ComparisonService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.loadCategories();
    this.loadProducts();

    if (this.isLoggedIn) {
      this.loadWishlist();
    }

    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isLoggedIn = !!user;
        if (user) {
          this.loadWishlist();
        }
      });
  }

  loadCategories(): void {
    this.categoryService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe(categories => {
        this.categories = categories;
      });
  }

  loadProducts(): void {
    const params: ProductQueryParams = {
      searchTerm: this.searchTerm || undefined,
      categoryId: this.selectedCategory || undefined,
      pageNumber: this.currentPage,
      pageSize: 12
    };

    // Add cache-bypass headers to ensure fresh data
    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache'
    });

    this.productService.getProducts(params, headers)
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        this.products = result.items;
        this.totalPages = result.totalPages;
        this.loading = false;
      });
  }

  loadWishlist(): void {
    if (!this.isLoggedIn) {
      return;
    }

    this.wishlistService.getWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (wishlist) => {
          this.wishlistProductIds = new Set(wishlist.items.map(item => item.productId));
        },
        error: () => {
          // Ignore errors for guest users
          this.wishlistProductIds = new Set();
        }
      });
  }

  toggleWishlist(product: Product): void {
    if (!this.isLoggedIn) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.isInWishlist(product.id)) {
      // Find wishlist item and remove it - use take(1) for current value
      this.wishlistService.wishlist$
        .pipe(take(1))
        .subscribe(wishlist => {
          if (wishlist) {
            const item = wishlist.items.find(i => i.productId === product.id);
            if (item) {
              this.wishlistService.removeFromWishlist(item.id)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                  next: () => {
                    this.wishlistProductIds.delete(product.id);
                  }
                });
            }
          }
        });
    } else {
      this.wishlistService.addToWishlist({ productId: product.id })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.wishlistProductIds.add(product.id);
          },
          error: (error) => {
            this.toastService.error('Wishlist Error', error.error?.message || 'Failed to add to wishlist');
          }
        });
    }
  }

  isInWishlist(productId: number): boolean {
    return this.wishlistProductIds.has(productId);
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.loadProducts();
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/600x400/CCCCCC/FFFFFF?text=No+Image';
    }

    // If it's already a full URL, return as is
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }

    // Otherwise, construct the full URL from the backend (remove /api from base URL)
    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }

  toggleComparison(product: Product): void {
    if (this.isInComparison(product.id)) {
      this.comparisonService.removeProduct(product.id);
    } else {
      const added = this.comparisonService.addProduct(product);
      if (!added) {
        this.toastService.warning('Comparison Limit', 'Maximum 4 products can be compared at once. Please remove a product first.');
      }
    }
  }

  isInComparison(productId: number): boolean {
    return this.comparisonService.isInComparison(productId);
  }

  getComparisonCount(): number {
    return this.comparisonService.getCount();
  }

  getCompareButtonTitle(productId: number): string {
    if (this.isInComparison(productId)) {
      return 'Remove from comparison';
    } else if (this.getComparisonCount() >= 4) {
      return 'Maximum 4 products can be compared';
    } else {
      return 'Add to comparison';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
