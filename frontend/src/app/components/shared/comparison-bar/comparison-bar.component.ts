import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ComparisonService } from '../../../services/comparison.service';
import { Product } from '../../../models/product.models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-comparison-bar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './comparison-bar.component.html',
  styleUrls: ['./comparison-bar.component.css']
})
export class ComparisonBarComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  isExpanded = false;
  private destroy$ = new Subject<void>();

  constructor(
    private comparisonService: ComparisonService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.comparisonService.comparison$
      .pipe(takeUntil(this.destroy$))
      .subscribe(products => {
        this.products = products;
        // Auto-collapse if products are removed
        if (products.length === 0) {
          this.isExpanded = false;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleExpanded(): void {
    this.isExpanded = !this.isExpanded;
  }

  removeProduct(productId: number, event: Event): void {
    event.stopPropagation();
    this.comparisonService.removeProduct(productId);
  }

  clearAll(event: Event): void {
    event.stopPropagation();
    this.comparisonService.clearAll();
  }

  goToComparison(): void {
    this.router.navigate(['/comparison']);
  }

  getProductCount(): number {
    return this.products.length;
  }

  getMaxProducts(): number {
    return 4;
  }

  getRemainingSlots(): number {
    return this.getMaxProducts() - this.getProductCount();
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return 'https://placehold.co/60x60/CCCCCC/FFFFFF?text=No+Image';
    }

    // If it's already a full URL, return as is
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }

    // Otherwise, construct the full URL from the backend (remove /api from base URL)
    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }
}
