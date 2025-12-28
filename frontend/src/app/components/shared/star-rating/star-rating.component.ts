import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './star-rating.component.html',
  styleUrls: ['./star-rating.component.scss']
})
export class StarRatingComponent {
  @Input() rating: number | null | undefined = 0;
  @Input() reviewCount: number | undefined = 0;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() showValue: boolean = true;
  @Input() showCount: boolean = true;

  stars = [1, 2, 3, 4, 5];

  get displayRating(): number {
    return this.rating || 0;
  }

  get displayReviewCount(): number {
    return this.reviewCount || 0;
  }

  get formattedRating(): string {
    return this.rating ? this.rating.toFixed(1) : 'N/A';
  }
}
