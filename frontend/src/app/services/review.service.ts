import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Review,
  ProductRating,
  CreateReviewRequest,
  UpdateReviewRequest,
  ReviewVoteRequest,
  ReviewModerationRequest,
  ReviewFilterRequest,
  ReviewListResponse
} from '../models/review.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  // Create a new review
  createReview(request: CreateReviewRequest): Observable<Review> {
    return this.http.post<Review>(this.apiUrl, request);
  }

  // Get a specific review by ID
  getReviewById(id: number): Observable<Review> {
    return this.http.get<Review>(`${this.apiUrl}/${id}`);
  }

  // Update a review
  updateReview(id: number, request: UpdateReviewRequest): Observable<Review> {
    return this.http.put<Review>(`${this.apiUrl}/${id}`, request);
  }

  // Delete a review
  deleteReview(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Get reviews for a specific product
  getProductReviews(productId: number, filter?: ReviewFilterRequest): Observable<ReviewListResponse> {
    let params = new HttpParams();

    if (filter) {
      if (filter.rating) params = params.set('rating', filter.rating.toString());
      if (filter.verifiedPurchasesOnly) params = params.set('verifiedPurchasesOnly', filter.verifiedPurchasesOnly.toString());
      if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
      if (filter.pageNumber) params = params.set('pageNumber', filter.pageNumber.toString());
      if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<ReviewListResponse>(`${this.apiUrl}/product/${productId}`, { params });
  }

  // Get product rating summary
  getProductRating(productId: number): Observable<ProductRating> {
    return this.http.get<ProductRating>(`${this.apiUrl}/product/${productId}/rating`);
  }

  // Get current user's reviews
  getMyReviews(): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/my-reviews`);
  }

  // Check if user has reviewed a product
  hasUserReviewedProduct(productId: number): Observable<{ hasReviewed: boolean }> {
    return this.http.get<{ hasReviewed: boolean }>(`${this.apiUrl}/product/${productId}/has-reviewed`);
  }

  // Vote on a review (helpful/unhelpful)
  voteReview(request: ReviewVoteRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/vote`, request);
  }

  // Remove vote from a review
  removeVote(reviewId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/vote/${reviewId}`);
  }

  // Admin: Get all reviews with optional approved filter
  getAllReviews(approved?: boolean, pageNumber: number = 1, pageSize: number = 10): Observable<ReviewListResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (approved !== undefined) {
      params = params.set('approved', approved.toString());
    }

    return this.http.get<ReviewListResponse>(`${this.apiUrl}/admin/all`, { params });
  }

  // Admin: Get pending reviews
  getPendingReviews(): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/admin/pending`);
  }

  // Admin: Moderate a review (approve/reject, feature)
  moderateReview(id: number, request: ReviewModerationRequest): Observable<Review> {
    return this.http.put<Review>(`${this.apiUrl}/${id}/moderate`, request);
  }
}
