export enum DiscountType {
  Percentage = 1,
  FixedAmount = 2,
  FreeShipping = 3,
  BuyXGetY = 4
}

export interface DiscountCode {
  id: number;
  code: string;
  discountType: DiscountType;
  value: number;
  minimumPurchase?: number;
  maximumDiscount?: number;
  validFrom: Date;
  validTo: Date;
  totalUsageLimit?: number;
  usedCount: number;
  perUserLimit?: number;
  buyQuantity?: number;
  getQuantity?: number;
  targetProductId?: number;
  targetProductName?: string;
  isActive: boolean;
  description?: string;
  createdAt: Date;
  applicableProductIds: number[];
  applicableCategoryIds: number[];
  isExpired: boolean;
  isUsageLimitReached: boolean;
  isValid: boolean;
}

export interface CreateDiscountCodeRequest {
  code: string;
  discountType: DiscountType;
  value: number;
  minimumPurchase?: number;
  maximumDiscount?: number;
  validFrom: Date;
  validTo: Date;
  totalUsageLimit?: number;
  perUserLimit?: number;
  buyQuantity?: number;
  getQuantity?: number;
  targetProductId?: number;
  isActive: boolean;
  description?: string;
  applicableProductIds: number[];
  applicableCategoryIds: number[];
}

export interface UpdateDiscountCodeRequest {
  description?: string;
  validFrom?: Date;
  validTo?: Date;
  isActive?: boolean;
  totalUsageLimit?: number;
  applicableProductIds?: number[];
  applicableCategoryIds?: number[];
}

export interface ApplyCouponRequest {
  couponCode: string;
  sessionId?: string;
}

export interface CouponValidationResponse {
  isValid: boolean;
  errorMessage?: string;
  couponCode: string;
  discountType?: DiscountType;
  discountValue: number;
  discountAmount: number;
  cartSubTotal: number;
  finalTotal: number;
  eligibleProductIds: number[];
  successMessage?: string;
  freeItemsAdded?: number;
  freeItemProductName?: string;
}

export interface DiscountCodeStats {
  totalCodes: number;
  activeCodes: number;
  expiredCodes: number;
  totalUsages: number;
  totalDiscountGiven: number;
}
