// Review DTOs
export interface Review {
  reviewId: number;
  productId: number;
  productName: string;
  userId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  isApproved: boolean;
  isFeatured: boolean;
  helpfulVotes: number;
  unhelpfulVotes: number;
  createdAt: Date;
  updatedAt?: Date;
  currentUserVote?: boolean; // null if not voted, true if helpful, false if unhelpful
}

export interface ReviewSummary {
  reviewId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  helpfulVotes: number;
  totalVotes: number;
  createdAt: Date;
}

export interface ProductRating {
  productId: number;
  averageRating: number;
  totalReviews: number;
  distribution: RatingDistribution;
}

export interface RatingDistribution {
  fiveStars: number;
  fourStars: number;
  threeStars: number;
  twoStars: number;
  oneStar: number;
  fiveStarsPercentage: number;
  fourStarsPercentage: number;
  threeStarsPercentage: number;
  twoStarsPercentage: number;
  oneStarPercentage: number;
}

// Request DTOs
export interface CreateReviewRequest {
  productId: number;
  rating: number;
  title: string;
  comment: string;
  orderId?: number;
}

export interface UpdateReviewRequest {
  rating: number;
  title: string;
  comment: string;
}

export interface ReviewVoteRequest {
  reviewId: number;
  isHelpful: boolean;
}

export interface ReviewModerationRequest {
  isApproved: boolean;
  isFeatured?: boolean;
}

export interface ReviewFilterRequest {
  rating?: number;
  verifiedPurchasesOnly?: boolean;
  sortBy?: 'MostRecent' | 'MostHelpful' | 'HighestRating' | 'LowestRating';
  pageNumber?: number;
  pageSize?: number;
}

// Response DTOs
export interface ReviewListResponse {
  reviews: Review[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
