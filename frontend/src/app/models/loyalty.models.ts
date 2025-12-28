// Enums
export enum LoyaltyTier {
  Bronze = 'Bronze',
  Silver = 'Silver',
  Gold = 'Gold',
  Platinum = 'Platinum'
}

export enum RewardType {
  PercentageDiscount = 'PercentageDiscount',
  FixedDiscount = 'FixedDiscount',
  FreeShipping = 'FreeShipping',
  FreeProduct = 'FreeProduct'
}

// Loyalty Account DTOs
export interface LoyaltyAccount {
  id: number;
  userId: number;
  userName: string;
  currentPoints: number;
  lifetimePoints: number;
  tier: string;
  tierBenefits: string;
  pointsToNextTier: number;
  earningMultiplier: number;
  createdAt: Date;
}

export interface LoyaltyTransaction {
  id: number;
  type: string;
  points: number;
  description: string;
  orderId?: number;
  orderNumber?: string;
  createdAt: Date;
}

export interface LoyaltyReward {
  id: number;
  name: string;
  description: string;
  pointsCost: number;
  type: string;
  discountPercentage?: number;
  discountAmount?: number;
  isFreeShipping: boolean;
  minimumTier?: string;
  canRedeem: boolean;
}

export interface RedeemedReward {
  id: number;
  rewardName: string;
  redemptionCode: string;
  pointsSpent: number;
  isUsed: boolean;
  redeemedAt: Date;
  expiresAt: Date;
}

export interface RedeemRewardRequest {
  rewardId: number;
}

export interface RedeemRewardResponse {
  success: boolean;
  message: string;
  redemptionCode?: string;
  pointsSpent: number;
  remainingPoints: number;
  expiresAt?: Date;
}

export interface LoyaltySummary {
  currentPoints: number;
  lifetimePoints: number;
  tier: string;
  pointsToNextTier: number;
  nextTier: string;
  earningMultiplier: number;
  tierBenefits: string[];
  recentTransactions: LoyaltyTransaction[];
  activeRewards: RedeemedReward[];
}

export interface EarnPointsRequest {
  userId: number;
  orderId: number;
  orderTotal: number;
}

export interface AdjustPointsRequest {
  userId: number;
  points: number;
  reason: string;
}

export interface CreateRewardRequest {
  name: string;
  description: string;
  pointsCost: number;
  type: RewardType;
  discountPercentage?: number;
  discountAmount?: number;
  isFreeShipping: boolean;
  minimumTier?: LoyaltyTier;
}
