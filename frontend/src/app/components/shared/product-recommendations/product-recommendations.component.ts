import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Product } from '../../../models/product.models';
import { StarRatingComponent } from '../star-rating/star-rating.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-recommendations',
  standalone: true,
  imports: [CommonModule, RouterLink, StarRatingComponent],
  templateUrl: './product-recommendations.component.html',
  styleUrls: ['./product-recommendations.component.scss']
})
export class ProductRecommendationsComponent implements OnInit {
  @Input() products: Product[] = [];
  @Input() title: string = 'Recommended for You';

  ngOnInit(): void {}

  getProductPrice(product: Product): number {
    return product.discountPrice || product.price;
  }

  hasDiscount(product: Product): boolean {
    return !!product.discountPrice && product.discountPrice < product.price;
  }

  getDiscountPercentage(product: Product): number {
    if (!this.hasDiscount(product)) return 0;
    return Math.round(((product.price - product.discountPrice!) / product.price) * 100);
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
