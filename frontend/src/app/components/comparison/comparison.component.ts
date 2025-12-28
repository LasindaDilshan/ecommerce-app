import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ComparisonService } from '../../services/comparison.service';
import { Product } from '../../models/product.models';
import { StarRatingComponent } from '../shared/star-rating/star-rating.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-comparison',
  standalone: true,
  imports: [CommonModule, RouterModule, StarRatingComponent],
  templateUrl: './comparison.component.html',
  styleUrls: ['./comparison.component.css']
})
export class ComparisonComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  private destroy$ = new Subject<void>();

  // Comparison attributes to display
  comparisonAttributes = [
    { key: 'imageUrl', label: 'Product Image', type: 'image' },
    { key: 'name', label: 'Product Name', type: 'text' },
    { key: 'price', label: 'Price', type: 'currency' },
    { key: 'discountPrice', label: 'Discount Price', type: 'currency' },
    { key: 'rating', label: 'Rating', type: 'rating' },
    { key: 'reviewCount', label: 'Reviews', type: 'number' },
    { key: 'categoryName', label: 'Category', type: 'text' },
    { key: 'stockQuantity', label: 'In Stock', type: 'stock' },
    { key: 'description', label: 'Description', type: 'text' }
  ];

  constructor(
    private comparisonService: ComparisonService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.comparisonService.comparison$
      .pipe(takeUntil(this.destroy$))
      .subscribe(products => {
        this.products = products;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  removeProduct(productId: number): void {
    this.comparisonService.removeProduct(productId);
  }

  clearAll(): void {
    if (confirm('Are you sure you want to clear all products from comparison?')) {
      this.comparisonService.clearAll();
      this.router.navigate(['/products']);
    }
  }

  addToCart(product: Product): void {
    // Navigate to product detail to add to cart
    this.router.navigate(['/products', product.id]);
  }

  viewProduct(productId: number): void {
    this.router.navigate(['/products', productId]);
  }

  getAttributeValue(product: Product, key: string): any {
    return (product as any)[key];
  }

  formatPrice(price: number | null | undefined): string {
    if (price === null || price === undefined) return 'N/A';
    return `$${price.toFixed(2)}`;
  }

  getEffectivePrice(product: Product): number {
    return product.discountPrice || product.price;
  }

  getSavings(product: Product): number | null {
    if (product.discountPrice && product.discountPrice < product.price) {
      return product.price - product.discountPrice;
    }
    return null;
  }

  getSavingsPercentage(product: Product): number | null {
    const savings = this.getSavings(product);
    if (savings) {
      return Math.round((savings / product.price) * 100);
    }
    return null;
  }

  getStockStatus(quantity: number): string {
    if (quantity === 0) return 'Out of Stock';
    if (quantity < 10) return `Only ${quantity} left`;
    return 'In Stock';
  }

  getStockClass(quantity: number): string {
    if (quantity === 0) return 'out-of-stock';
    if (quantity < 10) return 'low-stock';
    return 'in-stock';
  }

  getLowestPrice(): number {
    if (this.products.length === 0) return 0;
    return Math.min(...this.products.map(p => this.getEffectivePrice(p)));
  }

  isLowestPrice(product: Product): boolean {
    return this.getEffectivePrice(product) === this.getLowestPrice();
  }

  getHighestRating(): number {
    if (this.products.length === 0) return 0;
    return Math.max(...this.products.map(p => p.rating || 0));
  }

  isHighestRating(product: Product): boolean {
    return (product.rating || 0) === this.getHighestRating();
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
