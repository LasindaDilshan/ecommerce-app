import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, take } from 'rxjs/operators';
import { ProductService } from '../../../services/product.service';
import { CartService } from '../../../services/cart.service';
import { WishlistService } from '../../../services/wishlist.service';
import { AuthService } from '../../../services/auth.service';
import { RecentlyViewedService } from '../../../services/recently-viewed.service';
import { ComparisonService } from '../../../services/comparison.service';
import { ToastService } from '../../../services/toast.service';
import { Product } from '../../../models/product.models';
import { ProductRecommendationsComponent } from '../../shared/product-recommendations/product-recommendations.component';
import { ImageZoomComponent } from '../../shared/image-zoom/image-zoom.component';
import { StarRatingComponent } from '../../shared/star-rating/star-rating.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, ProductRecommendationsComponent, ImageZoomComponent, StarRatingComponent],
  template: `
    <div class="container" *ngIf="product">
      <div class="product-detail">
        <app-image-zoom
          [images]="getProductImages()"
          [productName]="product.name">
        </app-image-zoom>
        <div class="product-info">
          <h1>{{ product.name }}</h1>
          <div class="rating-section">
            <app-star-rating
              [rating]="product.rating"
              [reviewCount]="product.reviewCount"
              [size]="'medium'">
            </app-star-rating>
          </div>

          <!-- Urgency Indicators -->
          <div class="urgency-indicators">
            <!-- Low Stock Warning -->
            <div class="urgency-badge low-stock" *ngIf="isLowStock()">
              ⚠️ Only {{ product.stockQuantity }} left in stock!
            </div>

            <!-- Viewers Count -->
            <div class="urgency-badge viewers">
              👁️ {{ viewersCount }} people are viewing this right now
            </div>

            <!-- Flash Sale Timer (if discount exists) -->
            <div class="urgency-badge flash-sale" *ngIf="product.discountPrice && flashSaleEndTime">
              🔥 Flash Sale ends in: {{ flashSaleTimeRemaining }}
            </div>
          </div>

          <div class="price-section">
            <span *ngIf="product.discountPrice" class="original-price">\${{ product.price }}</span>
            <span class="current-price">\${{ product.discountPrice || product.price }}</span>
            <span *ngIf="product.discountPrice" class="discount-badge">
              {{ ((product.price - product.discountPrice) / product.price * 100).toFixed(0) }}% OFF
            </span>
          </div>
          <p class="description">{{ product.description }}</p>
          <p class="stock" [class.out-of-stock]="product.stockQuantity === 0">
            {{ product.stockQuantity > 0 ? 'In Stock (' + product.stockQuantity + ' available)' : 'Out of Stock' }}
          </p>
          <div class="actions">
            <button (click)="addToCart()" [disabled]="product.stockQuantity === 0" class="btn btn-primary">
              Add to Cart
            </button>
            <button
              *ngIf="isLoggedIn"
              (click)="toggleWishlist()"
              class="btn"
              [class.btn-danger]="isInWishlist"
              [class.btn-secondary]="!isInWishlist">
              {{ isInWishlist ? '❤ Remove from Wishlist' : '🤍 Add to Wishlist' }}
            </button>
            <button
              (click)="toggleComparison()"
              class="btn"
              [class.btn-success]="isInComparison()"
              [class.btn-secondary]="!isInComparison()"
              [disabled]="!isInComparison() && getComparisonCount() >= 4">
              {{ isInComparison() ? '⚖️ Remove from Compare' : '⚖️ Add to Compare' }}
            </button>
          </div>
        </div>
      </div>

      <app-product-recommendations
        [products]="customersAlsoBought"
        [title]="'Customers Also Bought'">
      </app-product-recommendations>

      <app-product-recommendations
        [products]="similarProducts"
        [title]="'Similar Products'">
      </app-product-recommendations>
    </div>
  `,
  styles: [`
    .product-detail { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; padding: 40px 0; }
    .product-image-container { border-radius: 8px; overflow: hidden; }
    .product-detail img { width: 100%; height: 400px; object-fit: cover; }
    .product-info h1 { margin-bottom: 15px; font-size: 2rem; }
    .rating-section { margin-bottom: 20px; }

    .urgency-indicators { margin-bottom: 20px; display: flex; flex-direction: column; gap: 10px; }
    .urgency-badge { padding: 10px 15px; border-radius: 6px; font-size: 14px; font-weight: 600; display: inline-flex; align-items: center; animation: pulse 2s ease-in-out infinite; }
    .urgency-badge.low-stock { background: #fff3cd; color: #856404; border: 2px solid #ffc107; }
    .urgency-badge.viewers { background: #e7f3ff; color: #004085; border: 2px solid #b8daff; }
    .urgency-badge.flash-sale { background: #f8d7da; color: #721c24; border: 2px solid #f5c6cb; }
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.85; }
    }

    .price-section { margin-bottom: 20px; display: flex; align-items: center; gap: 15px; }
    .original-price { font-size: 20px; text-decoration: line-through; color: #999; }
    .current-price { font-size: 32px; font-weight: bold; color: #007bff; }
    .discount-badge { background: #28a745; color: white; padding: 5px 10px; border-radius: 4px; font-size: 14px; font-weight: bold; }
    .description { font-size: 16px; line-height: 1.6; margin-bottom: 20px; color: #666; }
    .stock { font-size: 16px; font-weight: 500; margin-bottom: 30px; color: #28a745; }
    .stock.out-of-stock { color: #dc3545; }
    .actions { display: flex; gap: 15px; }
    .actions button { padding: 12px 30px; font-size: 16px; flex: 1; }
    @media (max-width: 768px) {
      .product-detail { grid-template-columns: 1fr; }
    }
  `]
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  product: Product | null = null;
  isInWishlist = false;
  isLoggedIn = false;
  similarProducts: Product[] = [];
  customersAlsoBought: Product[] = [];

  // Urgency indicators
  viewersCount: number = 0;
  flashSaleEndTime: Date | null = null;
  flashSaleTimeRemaining: string = '';
  private timerInterval: any;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private recentlyViewedService: RecentlyViewedService,
    private comparisonService: ComparisonService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();

    const id = Number(this.route.snapshot.paramMap.get('id'));
    // Add cache-bypass headers to ensure fresh data
    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache'
    });
    this.productService.getProductById(id, headers)
      .pipe(takeUntil(this.destroy$))
      .subscribe(product => {
        this.product = product;

        // Add to recently viewed products
        this.recentlyViewedService.addProduct(product);

        // Initialize urgency indicators
        this.initUrgencyIndicators();

        if (this.isLoggedIn) {
          this.checkWishlistStatus();
        }
        // Load recommendations
        this.loadRecommendations(id);
      });

    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isLoggedIn = !!user;
        if (user && this.product) {
          this.checkWishlistStatus();
        }
      });
  }

  loadRecommendations(productId: number): void {
    // Load similar products
    this.productService.getSimilarProducts(productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products) => {
          this.similarProducts = products;
        },
        error: (err) => console.error('Failed to load similar products:', err)
      });

    // Load customers also bought
    this.productService.getCustomersAlsoBought(productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products) => {
          this.customersAlsoBought = products;
        },
        error: (err) => console.error('Failed to load recommendations:', err)
      });
  }

  checkWishlistStatus(): void {
    if (this.product) {
      this.wishlistService.wishlist$
        .pipe(takeUntil(this.destroy$))
        .subscribe(wishlist => {
          if (wishlist) {
            this.isInWishlist = wishlist.items.some(item => item.productId === this.product!.id);
          }
        });
    }
  }

  toggleWishlist(): void {
    if (!this.isLoggedIn || !this.product) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.isInWishlist) {
      // Find wishlist item and remove it - use take(1) to get current value only
      this.wishlistService.wishlist$
        .pipe(take(1))
        .subscribe(wishlist => {
          if (wishlist) {
            const item = wishlist.items.find(i => i.productId === this.product!.id);
            if (item) {
              this.wishlistService.removeFromWishlist(item.id).subscribe({
                next: () => {
                  this.isInWishlist = false;
                },
                error: (error) => {
                  console.error('Failed to remove from wishlist:', error);
                }
              });
            }
          }
        });
    } else {
      this.wishlistService.addToWishlist({ productId: this.product.id }).subscribe({
        next: () => {
          this.isInWishlist = true;
        },
        error: (error) => {
          console.error('Failed to add to wishlist:', error.error?.message || error);
        }
      });
    }
  }

  addToCart(): void {
    if (this.product) {
      this.cartService.addToCart({ productId: this.product.id, quantity: 1 }).subscribe({
        next: () => {
          this.toastService.success('Added to Cart', 'Product has been added to your cart');
        },
        error: () => this.router.navigate(['/login'])
      });
    }
  }

  // Urgency Indicators
  isLowStock(): boolean {
    return this.product !== null && this.product.stockQuantity > 0 && this.product.stockQuantity < 10;
  }

  initUrgencyIndicators(): void {
    // Generate random viewers count (5-25)
    this.viewersCount = Math.floor(Math.random() * 20) + 5;

    // If product has discount, set flash sale end time (2 hours from now)
    if (this.product?.discountPrice) {
      this.flashSaleEndTime = new Date(Date.now() + 2 * 60 * 60 * 1000);
      this.updateFlashSaleTimer();

      // Update timer every second
      this.timerInterval = setInterval(() => {
        this.updateFlashSaleTimer();
      }, 1000);
    }
  }

  updateFlashSaleTimer(): void {
    if (!this.flashSaleEndTime) return;

    const now = new Date().getTime();
    const distance = this.flashSaleEndTime.getTime() - now;

    if (distance < 0) {
      this.flashSaleTimeRemaining = 'EXPIRED';
      if (this.timerInterval) {
        clearInterval(this.timerInterval);
      }
      return;
    }

    const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    this.flashSaleTimeRemaining = `${hours}h ${minutes}m ${seconds}s`;
  }

  getProductImages(): string[] {
    if (!this.product) return [];
    const images = [this.getImageUrl(this.product.imageUrl)];
    if (this.product.additionalImages && this.product.additionalImages.length > 0) {
      images.push(...this.product.additionalImages.map(img => this.getImageUrl(img)));
    }
    return images;
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

  toggleComparison(): void {
    if (!this.product) return;

    if (this.isInComparison()) {
      this.comparisonService.removeProduct(this.product.id);
    } else {
      const added = this.comparisonService.addProduct(this.product);
      if (!added) {
        this.toastService.warning('Comparison Limit', 'Maximum 4 products can be compared at once. Please remove a product first.');
      }
    }
  }

  isInComparison(): boolean {
    if (!this.product) return false;
    return this.comparisonService.isInComparison(this.product.id);
  }

  getComparisonCount(): number {
    return this.comparisonService.getCount();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }
}
