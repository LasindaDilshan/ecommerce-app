// Product Question DTOs
export interface ProductQuestion {
  id: number;
  productId: number;
  productName: string;
  userId: number;
  userName: string;
  questionText: string;
  isApproved: boolean;
  isAnswered: boolean;
  upvoteCount: number;
  answerCount: number;
  createdAt: Date;
  answers: ProductAnswer[];
  hasUserVoted: boolean;
}

export interface ProductAnswer {
  id: number;
  questionId: number;
  userId: number;
  userName: string;
  answerText: string;
  isApproved: boolean;
  isVerifiedPurchase: boolean;
  isSellerAnswer: boolean;
  isAccepted: boolean;
  helpfulCount: number;
  createdAt: Date;
  hasUserVoted: boolean;
}

// Request DTOs
export interface CreateQuestionRequest {
  productId: number;
  questionText: string;
}

export interface CreateAnswerRequest {
  questionId: number;
  answerText: string;
}

export interface ModerateQuestionRequest {
  isApproved: boolean;
}

export interface ModerateAnswerRequest {
  isApproved: boolean;
  isAccepted?: boolean;
}

// Response DTOs
export interface QuestionListResponse {
  questions: ProductQuestion[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface VoteResponse {
  success: boolean;
  message: string;
  newCount: number;
}

// Query Parameters
export interface QuestionQueryParams {
  productId?: number;
  userId?: number;
  isApproved?: boolean;
  isAnswered?: boolean;
  sortBy?: 'MostRecent' | 'MostVoted' | 'MostAnswered';
  pageNumber?: number;
  pageSize?: number;
}
