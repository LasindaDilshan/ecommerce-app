import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpHeaders } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, take } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { WishlistService } from '../../services/wishlist.service';
import { AuthService } from '../../services/auth.service';
import { RecentlyViewedService } from '../../services/recently-viewed.service';
import { ToastService } from '../../services/toast.service';
import { Product } from '../../models/product.models';
import { environment } from '../../../environments/environment';
import { ProductRecommendationsComponent } from '../shared/product-recommendations/product-recommendations.component';
import { StarRatingComponent } from '../shared/star-rating/star-rating.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductRecommendationsComponent, StarRatingComponent],
  template: `
    <div class="hero">
      <div class="container">
        <h1>Welcome to Our E-Commerce Store</h1>
        <p>Find the best products at amazing prices</p>
        <a routerLink="/products" class="btn btn-primary">Shop Now</a>
      </div>
    </div>

    <div class="container">
      <h2 class="section-title">Featured Products</h2>

      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
      </div>

      <div class="product-grid" *ngIf="!loading">
        <div class="product-card" *ngFor="let product of featuredProducts">
          <div class="product-image-container">
            <img
              [src]="getImageUrl(product.imageUrl)"
              [alt]="product.name"
              class="product-image"
            />
            <button
              *ngIf="isLoggedIn"
              (click)="toggleWishlist(product)"
              class="wishlist-btn"
              [class.in-wishlist]="isInWishlist(product.id)">
              ❤
            </button>
          </div>
          <div class="product-info">
            <h3>{{ product.name }}</h3>
            <div class="rating-wrapper">
              <app-star-rating
                [rating]="product.rating"
                [reviewCount]="product.reviewCount"
                [size]="'small'"
                [showValue]="false">
              </app-star-rating>
            </div>
            <p class="product-price">
              <span *ngIf="product.discountPrice" class="original-price">\${{ product.price }}</span>
              <span class="current-price">\${{ product.discountPrice || product.price }}</span>
            </p>
            <div class="product-actions">
              <a [routerLink]="['/products', product.id]" class="btn btn-secondary">View Details</a>
              <button (click)="addToCart(product)" class="btn btn-primary">Add to Cart</button>
            </div>
          </div>
        </div>
      </div>

      <div class="container">
        <app-product-recommendations
          [products]="recentlyViewedProducts"
          [title]="'Recently Viewed'">
        </app-product-recommendations>
      </div>
    </div>
  `,
  styles: [`
    .hero {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 80px 0;
      text-align: center;
    }

    .hero h1 {
      font-size: 48px;
      margin-bottom: 20px;
    }

    .hero p {
      font-size: 20px;
      margin-bottom: 30px;
    }

    .section-title {
      text-align: center;
      margin: 50px 0 30px;
    }

    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 30px;
      margin-bottom: 50px;
    }

    .product-card {
      background: white;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      transition: transform 0.3s ease;
    }

    .product-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 4px 8px rgba(0,0,0,0.2);
    }

    .product-image-container {
      position: relative;
    }

    .product-image {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }

    .wishlist-btn {
      position: absolute;
      top: 10px;
      right: 10px;
      background: white;
      border: 2px solid #ddd;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      font-size: 20px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      color: #ddd;
    }

    .wishlist-btn:hover {
      transform: scale(1.1);
      border-color: #ff6b6b;
    }

    .wishlist-btn.in-wishlist {
      background: #ff6b6b;
      border-color: #ff6b6b;
      color: white;
    }

    .product-info {
      padding: 20px;
    }

    .product-info h3 {
      margin-bottom: 10px;
    }

    .rating-wrapper {
      margin-bottom: 10px;
    }

    .product-price {
      margin-bottom: 15px;
    }

    .original-price {
      text-decoration: line-through;
      color: #999;
      margin-right: 10px;
    }

    .current-price {
      font-size: 24px;
      font-weight: bold;
      color: #007bff;
    }

    .product-actions {
      display: flex;
      gap: 10px;
    }

    .product-actions .btn {
      flex: 1;
      padding: 8px;
      font-size: 14px;
    }
  `]
})
export class HomeComponent implements OnInit, OnDestroy {
  featuredProducts: Product[] = [];
  recentlyViewedProducts: Product[] = [];
  loading = true;
  wishlistProductIds: Set<number> = new Set();
  isLoggedIn = false;
  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private router: Router,
    private recentlyViewedService: RecentlyViewedService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.loadFeaturedProducts();
    this.loadRecentlyViewed();

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

  loadRecentlyViewed(): void {
    this.recentlyViewedService.recentlyViewed$
      .pipe(takeUntil(this.destroy$))
      .subscribe(products => {
        this.recentlyViewedProducts = products;
      });
  }

  loadFeaturedProducts(): void {
    // Add cache-bypass headers to ensure fresh data
    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache'
    });
    this.productService.getFeaturedProducts(headers)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products) => {
          this.featuredProducts = products;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  loadWishlist(): void {
    this.wishlistService.getWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (wishlist) => {
          this.wishlistProductIds = new Set(wishlist.items.map(item => item.productId));
        },
        error: () => {
          // Ignore errors
        }
      });
  }

  toggleWishlist(product: Product): void {
    if (!this.isLoggedIn) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.isInWishlist(product.id)) {
      // Find wishlist item and remove it - use take(1) to get current value
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

  addToCart(product: Product): void {
    this.cartService.addToCart({ productId: product.id, quantity: 1 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Added to Cart', 'Product has been added to your cart');
        },
        error: () => {
          this.router.navigate(['/login']);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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
}
