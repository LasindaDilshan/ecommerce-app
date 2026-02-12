import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ReviewService } from '../../../services/review.service';
import { AuthService } from '../../../services/auth.service';
import { ToastService } from '../../../services/toast.service';
import { StarRatingComponent } from '../star-rating/star-rating.component';
import {
  Review, ProductRating, CreateReviewRequest,
  ReviewFilterRequest, ReviewListResponse
} from '../../../models/review.models';

@Component({
  selector: 'app-review-section',
  standalone: true,
  imports: [CommonModule, FormsModule, StarRatingComponent],
  template: `
    <div class="reviews-section">
      <h2>Customer Reviews</h2>

      <!-- Rating Summary -->
      <div class="rating-summary" *ngIf="productRating">
        <div class="overall-rating">
          <span class="big-rating">{{ productRating.averageRating.toFixed(1) }}</span>
          <app-star-rating [rating]="productRating.averageRating" [size]="'large'"></app-star-rating>
          <span class="total-reviews">{{ productRating.totalReviews }} reviews</span>
        </div>
        <div class="rating-bars">
          <div class="bar-row" *ngFor="let star of [5,4,3,2,1]">
            <span class="star-label">{{ star }} star</span>
            <div class="bar-track">
              <div class="bar-fill" [style.width.%]="getDistributionPercent(star)"></div>
            </div>
            <span class="bar-count">{{ getDistributionCount(star) }}</span>
          </div>
        </div>
      </div>

      <!-- Write Review -->
      <div class="write-review" *ngIf="isLoggedIn && !hasReviewed && !showReviewForm">
        <button (click)="showReviewForm = true" class="btn btn-primary">Write a Review</button>
      </div>

      <div class="review-form" *ngIf="showReviewForm">
        <h3>Write Your Review</h3>
        <div class="form-group">
          <label>Rating *</label>
          <div class="star-selector">
            <span *ngFor="let star of [1,2,3,4,5]"
              (click)="newReview.rating = star"
              class="selectable-star"
              [class.selected]="star <= newReview.rating">
              {{ star <= newReview.rating ? '\u2605' : '\u2606' }}
            </span>
          </div>
        </div>
        <div class="form-group">
          <label>Title *</label>
          <input type="text" [(ngModel)]="newReview.title" placeholder="Summary of your review" class="form-control" />
        </div>
        <div class="form-group">
          <label>Review *</label>
          <textarea [(ngModel)]="newReview.comment" placeholder="What did you like or dislike?" class="form-control" rows="4"></textarea>
        </div>
        <div class="form-actions">
          <button (click)="submitReview()" [disabled]="!canSubmitReview()" class="btn btn-primary">Submit Review</button>
          <button (click)="showReviewForm = false" class="btn btn-secondary">Cancel</button>
        </div>
      </div>

      <!-- Filters -->
      <div class="review-filters">
        <select [(ngModel)]="filterSortBy" (change)="loadReviews()" class="filter-select">
          <option value="MostRecent">Most Recent</option>
          <option value="MostHelpful">Most Helpful</option>
          <option value="HighestRating">Highest Rating</option>
          <option value="LowestRating">Lowest Rating</option>
        </select>
        <select [(ngModel)]="filterRating" (change)="loadReviews()" class="filter-select">
          <option [ngValue]="undefined">All Ratings</option>
          <option *ngFor="let r of [5,4,3,2,1]" [ngValue]="r">{{ r }} Stars</option>
        </select>
        <label class="checkbox-label">
          <input type="checkbox" [(ngModel)]="filterVerified" (change)="loadReviews()" />
          Verified Purchases Only
        </label>
      </div>

      <!-- Reviews List -->
      <div class="reviews-list">
        <div *ngFor="let review of reviews" class="review-card">
          <div class="review-header">
            <app-star-rating [rating]="review.rating" [size]="'small'"></app-star-rating>
            <strong class="review-title">{{ review.title }}</strong>
          </div>
          <div class="review-meta">
            <span class="reviewer">{{ review.userName }}</span>
            <span *ngIf="review.isVerifiedPurchase" class="verified-badge">Verified Purchase</span>
            <span class="review-date">{{ review.createdAt | date:'mediumDate' }}</span>
          </div>
          <p class="review-body">{{ review.comment }}</p>
          <div class="review-actions">
            <button (click)="voteReview(review.reviewId, true)" class="vote-btn" [class.voted]="review.currentUserVote === true">
              Helpful ({{ review.helpfulVotes }})
            </button>
            <button (click)="voteReview(review.reviewId, false)" class="vote-btn" [class.voted]="review.currentUserVote === false">
              Not Helpful ({{ review.unhelpfulVotes }})
            </button>
          </div>
        </div>

        <div *ngIf="reviews.length === 0" class="no-reviews">
          <p>No reviews yet. Be the first to review this product!</p>
        </div>

        <!-- Pagination -->
        <div class="pagination" *ngIf="totalPages > 1">
          <button (click)="goToPage(currentPage - 1)" [disabled]="currentPage <= 1" class="page-btn">Previous</button>
          <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
          <button (click)="goToPage(currentPage + 1)" [disabled]="currentPage >= totalPages" class="page-btn">Next</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .reviews-section { padding: 40px 0; border-top: 1px solid var(--border-color); margin-top: 40px; }
    .reviews-section h2 { font-size: 1.5rem; margin-bottom: 24px; color: var(--text-primary); }
    .rating-summary { display: flex; gap: 40px; margin-bottom: 30px; padding: 24px; background: var(--bg-card); border-radius: 12px; border: 1px solid var(--border-color); }
    .overall-rating { display: flex; flex-direction: column; align-items: center; gap: 8px; min-width: 150px; }
    .big-rating { font-size: 3rem; font-weight: 700; color: var(--text-primary); }
    .total-reviews { color: var(--text-secondary); font-size: 0.9rem; }
    .rating-bars { flex: 1; display: flex; flex-direction: column; gap: 8px; justify-content: center; }
    .bar-row { display: flex; align-items: center; gap: 12px; }
    .star-label { width: 50px; font-size: 0.85rem; color: var(--text-secondary); text-align: right; }
    .bar-track { flex: 1; height: 10px; background: var(--bg-secondary); border-radius: 5px; overflow: hidden; }
    .bar-fill { height: 100%; background: #fbbf24; border-radius: 5px; transition: width 0.3s; }
    .bar-count { width: 30px; font-size: 0.85rem; color: var(--text-secondary); }
    .write-review { margin-bottom: 24px; }
    .review-form { background: var(--bg-card); padding: 24px; border-radius: 12px; border: 1px solid var(--border-color); margin-bottom: 24px; }
    .review-form h3 { margin: 0 0 16px; color: var(--text-primary); }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 600; color: var(--text-primary); font-size: 0.9rem; }
    .form-control { width: 100%; padding: 10px 14px; border: 1px solid var(--border-color); border-radius: 8px; font-size: 1rem; background: var(--bg-secondary); color: var(--text-primary); box-sizing: border-box; }
    .form-control:focus { outline: none; border-color: var(--primary); }
    .star-selector { display: flex; gap: 4px; }
    .selectable-star { font-size: 2rem; cursor: pointer; color: #d1d5db; transition: color 0.15s; }
    .selectable-star.selected { color: #fbbf24; }
    .selectable-star:hover { color: #f59e0b; }
    .form-actions { display: flex; gap: 12px; }
    .review-filters { display: flex; gap: 12px; align-items: center; margin-bottom: 20px; flex-wrap: wrap; }
    .filter-select { padding: 8px 12px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-card); color: var(--text-primary); font-size: 0.9rem; }
    .checkbox-label { display: flex; align-items: center; gap: 6px; font-size: 0.9rem; color: var(--text-secondary); cursor: pointer; }
    .reviews-list { display: flex; flex-direction: column; gap: 16px; }
    .review-card { padding: 20px; background: var(--bg-card); border-radius: 10px; border: 1px solid var(--border-color); }
    .review-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
    .review-title { color: var(--text-primary); font-size: 1rem; }
    .review-meta { display: flex; gap: 12px; align-items: center; margin-bottom: 12px; flex-wrap: wrap; }
    .reviewer { font-weight: 600; color: var(--text-primary); font-size: 0.9rem; }
    .verified-badge { background: #d1fae5; color: #065f46; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 600; }
    .review-date { color: var(--text-tertiary); font-size: 0.85rem; }
    .review-body { color: var(--text-secondary); line-height: 1.6; margin: 0 0 12px; }
    .review-actions { display: flex; gap: 8px; }
    .vote-btn { padding: 4px 12px; border: 1px solid var(--border-color); border-radius: 6px; background: var(--bg-secondary); color: var(--text-secondary); cursor: pointer; font-size: 0.8rem; transition: all 0.2s; }
    .vote-btn:hover { border-color: var(--primary); color: var(--primary); }
    .vote-btn.voted { background: var(--primary); color: white; border-color: var(--primary); }
    .no-reviews { text-align: center; padding: 40px; color: var(--text-secondary); }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
    .page-btn { padding: 8px 16px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-card); color: var(--text-primary); cursor: pointer; }
    .page-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .page-info { color: var(--text-secondary); font-size: 0.9rem; }
    .btn { padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.95rem; }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: var(--bg-secondary); color: var(--text-primary); border: 1px solid var(--border-color); }
    @media (max-width: 768px) {
      .rating-summary { flex-direction: column; }
      .review-filters { flex-direction: column; align-items: flex-start; }
    }
  `]
})
export class ReviewSectionComponent implements OnInit, OnDestroy {
  @Input() productId!: number;

  reviews: Review[] = [];
  productRating: ProductRating | null = null;
  isLoggedIn = false;
  hasReviewed = false;
  showReviewForm = false;
  currentPage = 1;
  totalPages = 1;

  filterSortBy: string = 'MostRecent';
  filterRating: number | undefined;
  filterVerified = false;

  newReview: CreateReviewRequest = { productId: 0, rating: 0, title: '', comment: '' };

  private destroy$ = new Subject<void>();

  constructor(
    private reviewService: ReviewService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.newReview.productId = this.productId;
    this.loadRating();
    this.loadReviews();
    if (this.isLoggedIn) {
      this.checkIfReviewed();
    }
  }

  loadRating(): void {
    this.reviewService.getProductRating(this.productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (rating) => this.productRating = rating,
        error: () => {}
      });
  }

  loadReviews(): void {
    const filter: ReviewFilterRequest = {
      sortBy: this.filterSortBy as any,
      pageNumber: this.currentPage,
      pageSize: 10,
      rating: this.filterRating,
      verifiedPurchasesOnly: this.filterVerified || undefined
    };

    this.reviewService.getProductReviews(this.productId, filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.reviews = response.reviews;
          this.totalPages = response.totalPages;
        },
        error: () => {}
      });
  }

  checkIfReviewed(): void {
    this.reviewService.hasUserReviewedProduct(this.productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => this.hasReviewed = result.hasReviewed,
        error: () => {}
      });
  }

  canSubmitReview(): boolean {
    return this.newReview.rating > 0 && this.newReview.title.trim().length > 0 && this.newReview.comment.trim().length > 0;
  }

  submitReview(): void {
    if (!this.canSubmitReview()) return;

    this.reviewService.createReview(this.newReview)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Review Submitted', 'Your review has been submitted for approval.');
          this.showReviewForm = false;
          this.hasReviewed = true;
          this.newReview = { productId: this.productId, rating: 0, title: '', comment: '' };
          this.loadReviews();
          this.loadRating();
        },
        error: (err) => {
          this.toastService.error('Error', err.error?.message || 'Failed to submit review');
        }
      });
  }

  voteReview(reviewId: number, isHelpful: boolean): void {
    if (!this.isLoggedIn) return;

    this.reviewService.voteReview({ reviewId, isHelpful })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadReviews(),
        error: () => {}
      });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.loadReviews();
  }

  getDistributionPercent(star: number): number {
    if (!this.productRating?.distribution) return 0;
    const d = this.productRating.distribution;
    switch (star) {
      case 5: return d.fiveStarsPercentage;
      case 4: return d.fourStarsPercentage;
      case 3: return d.threeStarsPercentage;
      case 2: return d.twoStarsPercentage;
      case 1: return d.oneStarPercentage;
      default: return 0;
    }
  }

  getDistributionCount(star: number): number {
    if (!this.productRating?.distribution) return 0;
    const d = this.productRating.distribution;
    switch (star) {
      case 5: return d.fiveStars;
      case 4: return d.fourStars;
      case 3: return d.threeStars;
      case 2: return d.twoStars;
      case 1: return d.oneStar;
      default: return 0;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
