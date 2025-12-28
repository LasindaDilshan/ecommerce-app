import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WishlistService } from '../../services/wishlist.service';
import { CartService } from '../../services/cart.service';
import { Wishlist, WishlistItem } from '../../models/wishlist.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h1>My Wishlist</h1>

      <div *ngIf="wishlist && wishlist.items.length === 0" class="empty-wishlist">
        <p>Your wishlist is empty</p>
        <button (click)="continueShopping()" class="btn btn-primary">Continue Shopping</button>
      </div>

      <div *ngIf="wishlist && wishlist.items.length > 0" class="wishlist-content">
        <div class="wishlist-items">
          <div *ngFor="let item of wishlist.items" class="wishlist-item">
            <img [src]="getImageUrl(item.productImageUrl)" [alt]="item.productName" class="product-image">

            <div class="item-details">
              <h3>{{ item.productName }}</h3>
              <div class="price">
                <span *ngIf="item.productDiscountPrice" class="original-price">\${{ item.productPrice }}</span>
                <span class="current-price">\${{ item.productDiscountPrice || item.productPrice }}</span>
              </div>
              <p class="stock-status" [class.out-of-stock]="!item.isInStock">
                {{ item.isInStock ? 'In Stock' : 'Out of Stock' }}
              </p>
              <p class="added-date">Added: {{ item.addedAt | date:'short' }}</p>
            </div>

            <div class="item-actions">
              <button
                (click)="moveToCart(item)"
                [disabled]="!item.isInStock"
                class="btn btn-primary">
                Move to Cart
              </button>
              <button (click)="removeFromWishlist(item.id)" class="btn btn-danger">
                Remove
              </button>
            </div>
          </div>
        </div>

        <div class="wishlist-summary">
          <h3>Wishlist Summary</h3>
          <p>Total Items: {{ wishlist.itemCount }}</p>
          <button (click)="clearWishlist()" class="btn btn-secondary">Clear Wishlist</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    h1 {
      margin-bottom: 30px;
    }

    .empty-wishlist {
      text-align: center;
      padding: 60px 20px;
    }

    .empty-wishlist p {
      font-size: 1.2rem;
      margin-bottom: 20px;
      color: #666;
    }

    .wishlist-content {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 30px;
    }

    .wishlist-items {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .wishlist-item {
      display: flex;
      gap: 20px;
      padding: 20px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .product-image {
      width: 150px;
      height: 150px;
      object-fit: cover;
      border-radius: 4px;
    }

    .item-details {
      flex: 1;
    }

    .item-details h3 {
      margin: 0 0 10px 0;
      font-size: 1.2rem;
      color: #1f2937;
    }

    .price {
      margin-bottom: 10px;
    }

    .original-price {
      text-decoration: line-through;
      color: #9ca3af;
      margin-right: 10px;
    }

    .current-price {
      font-size: 1.3rem;
      font-weight: bold;
      color: #111827;
    }

    .stock-status {
      color: #059669;
      font-weight: 500;
      margin: 5px 0;
    }

    .stock-status.out-of-stock {
      color: #dc2626;
    }

    .added-date {
      color: #4b5563;
      font-size: 0.9rem;
    }

    .item-actions {
      display: flex;
      flex-direction: column;
      gap: 10px;
      justify-content: center;
    }

    .item-actions button {
      min-width: 120px;
    }

    .wishlist-summary {
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      height: fit-content;
    }

    .wishlist-summary h3 {
      margin-top: 0;
      margin-bottom: 15px;
      color: #1f2937;
    }

    .wishlist-summary p {
      margin-bottom: 20px;
      font-size: 1.1rem;
      color: #374151;
    }

    @media (max-width: 768px) {
      .wishlist-content {
        grid-template-columns: 1fr;
      }

      .wishlist-item {
        flex-direction: column;
      }

      .product-image {
        width: 100%;
        height: 200px;
      }

      .item-actions {
        flex-direction: row;
      }
    }
  `]
})
export class WishlistComponent implements OnInit, OnDestroy {
  wishlist: Wishlist | null = null;
  private destroy$ = new Subject<void>();

  // State for confirmation dialogs
  itemToRemove: number | null = null;
  showClearConfirm = false;

  constructor(
    private wishlistService: WishlistService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadWishlist();
  }

  loadWishlist(): void {
    this.wishlistService.getWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (wishlist) => {
          this.wishlist = wishlist;
        },
        error: (error) => {
          console.error('Error loading wishlist:', error);
        }
      });
  }

  moveToCart(item: WishlistItem): void {
    this.wishlistService.moveToCart({ wishlistItemId: item.id, quantity: 1 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadWishlist();
          this.cartService.getCart().pipe(takeUntil(this.destroy$)).subscribe();
        },
        error: (error) => {
          console.error('Error moving to cart:', error);
        }
      });
  }

  removeFromWishlist(wishlistItemId: number): void {
    this.wishlistService.removeFromWishlist(wishlistItemId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadWishlist();
        },
        error: (error) => {
          console.error('Error removing item:', error);
        }
      });
  }

  clearWishlist(): void {
    this.wishlistService.clearWishlist()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadWishlist();
        },
        error: (error) => {
          console.error('Error clearing wishlist:', error);
        }
      });
  }

  continueShopping(): void {
    this.router.navigate(['/products']);
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/150x150/CCCCCC/FFFFFF?text=No+Image';
    }
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
