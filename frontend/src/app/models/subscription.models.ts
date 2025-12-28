import { Address } from './order.models';
import { CartItem } from './cart.models';

// Enums
export enum SubscriptionInterval {
  Daily = 'Daily',
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Yearly = 'Yearly'
}

export enum SubscriptionStatus {
  Trial = 'Trial',
  Active = 'Active',
  Paused = 'Paused',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  PastDue = 'PastDue'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Completed = 'Completed',
  Failed = 'Failed',
  Refunded = 'Refunded'
}

export enum ReturnStatus {
  Requested = 'Requested',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Shipped = 'Shipped',
  Received = 'Received',
  Refunded = 'Refunded',
  Completed = 'Completed'
}

export enum ReturnReason {
  Defective = 'Defective',
  WrongItem = 'WrongItem',
  NotAsDescribed = 'NotAsDescribed',
  ChangedMind = 'ChangedMind',
  BetterPriceFound = 'BetterPriceFound',
  NoLongerNeeded = 'NoLongerNeeded',
  Other = 'Other'
}

export enum RefundMethod {
  OriginalPayment = 'OriginalPayment',
  StoreCredit = 'StoreCredit',
  GiftCard = 'GiftCard'
}

export enum ReturnItemCondition {
  New = 'New',
  Used = 'Used',
  Damaged = 'Damaged',
  Defective = 'Defective'
}

export enum GiftCardTransactionType {
  Created = 'Created',
  Redeemed = 'Redeemed',
  Refunded = 'Refunded',
  Expired = 'Expired'
}

// Subscription Plan DTOs
export interface SubscriptionPlan {
  planId: number;
  name: string;
  code: string;
  description: string;
  billingInterval: SubscriptionInterval;
  billingIntervalCount: number;
  price: number;
  setupFee?: number;
  trialPeriodDays?: number;
  isActive: boolean;
  features: string[];
  products: SubscriptionPlanProduct[];
}

export interface SubscriptionPlanProduct {
  productId: number;
  productName: string;
  productSKU: string;
  quantity: number;
  discountPercentage?: number;
  isOptional: boolean;
}

export interface CreateSubscriptionPlanRequest {
  name: string;
  code: string;
  description?: string;
  billingInterval: SubscriptionInterval;
  billingIntervalCount?: number;
  price: number;
  setupFee?: number;
  trialPeriodDays?: number;
  features?: string[];
  products?: AddProductToPlanRequest[];
}

export interface AddProductToPlanRequest {
  productId: number;
  quantity?: number;
  discountPercentage?: number;
  isOptional?: boolean;
}

// Subscription DTOs
export interface Subscription {
  subscriptionId: number;
  subscriptionNumber: string;
  userId: number;
  userEmail: string;
  planId: number;
  planName: string;
  status: SubscriptionStatus;
  currentPrice: number;
  startDate: Date;
  endDate?: Date;
  trialEndDate?: Date;
  nextBillingDate: Date;
  pausedUntil?: Date;
  cancelledAt?: Date;
  cardLast4?: string;
  cardBrand?: string;
  shippingAddress?: Address;
  recentPayments: SubscriptionPayment[];
  upcomingOrders: SubscriptionOrder[];
}

export interface CreateSubscriptionRequest {
  userId: number;
  planId: number;
  paymentMethodId?: string;
  shippingAddressId?: number;
  startTrial?: boolean;
}

export interface UpdateSubscriptionRequest {
  newPlanId?: number;
  shippingAddressId?: number;
  paymentMethodId?: string;
}

export interface PauseSubscriptionRequest {
  pauseUntil: Date;
  reason?: string;
}

export interface CancelSubscriptionRequest {
  cancelImmediately: boolean;
  reason?: string;
}

export interface SubscriptionPayment {
  paymentId: number;
  amount: number;
  status: PaymentStatus;
  paymentDate: Date;
  periodStartDate: Date;
  periodEndDate: Date;
  failureReason?: string;
}

export interface SubscriptionOrder {
  subscriptionOrderId: number;
  orderId: number;
  orderNumber: string;
  scheduledDate: Date;
  isSkipped: boolean;
  skipReason?: string;
}

// Returns & RMA DTOs
export interface ReturnRequest {
  returnId: number;
  returnNumber: string;
  orderId: number;
  orderNumber: string;
  status: ReturnStatus;
  reason: ReturnReason;
  comments?: string;
  requestDate: Date;
  approvedDate?: Date;
  refundAmount: number;
  refundMethod: RefundMethod;
  trackingNumber?: string;
  items: ReturnItem[];
}

export interface ReturnItem {
  productId: number;
  productName: string;
  productSKU: string;
  quantity: number;
  condition: ReturnItemCondition;
  refundAmount: number;
}

export interface CreateReturnRequest {
  orderId: number;
  reason: ReturnReason;
  comments?: string;
  items: ReturnItemRequest[];
}

export interface ReturnItemRequest {
  productId: number;
  quantity: number;
  condition: ReturnItemCondition;
}

export interface ProcessReturnRequest {
  approve: boolean;
  comments?: string;
  restockingFee?: number;
  refundMethod?: RefundMethod;
}

// Abandoned Cart DTOs
export interface AbandonedCart {
  abandonedCartId: number;
  userId?: number;
  userEmail?: string;
  guestEmail?: string;
  abandonedAt: Date;
  cartValue: number;
  itemCount: number;
  items: CartItem[];
  recoveryEmailsSent: number;
  isRecovered: boolean;
  recoveryCode?: string;
}

export interface RecoverAbandonedCartRequest {
  abandonedCartId: number;
  emailTemplate?: string;
  discountPercentage?: number;
}

// Gift Card DTOs
export interface GiftCard {
  giftCardId: number;
  code: string;
  initialValue: number;
  currentBalance: number;
  recipientEmail?: string;
  recipientName?: string;
  message?: string;
  isActive: boolean;
  expiresAt?: Date;
  transactions: GiftCardTransaction[];
}

export interface GiftCardTransaction {
  transactionId: number;
  type: GiftCardTransactionType;
  amount: number;
  balanceAfter: number;
  description?: string;
  transactionDate: Date;
}

export interface CreateGiftCardRequest {
  value: number;
  recipientEmail?: string;
  recipientName?: string;
  message?: string;
  expiresAt?: Date;
}

export interface RedeemGiftCardRequest {
  code: string;
  orderId?: number;
}

export interface GiftCardBalanceRequest {
  code: string;
}

// Analytics DTOs
export interface SalesAnalytics {
  totalRevenue: number;
  totalProfit: number;
  totalOrders: number;
  averageOrderValue: number;
  revenueByCategory: { [key: string]: number };
  ordersByStatus: { [key: string]: number };
  dailySales: DailySales[];
}

export interface DailySales {
  date: Date;
  revenue: number;
  orders: number;
  items: number;
}

export interface CustomerAnalytics {
  totalCustomers: number;
  newCustomers: number;
  returningCustomers: number;
  customerLifetimeValue: number;
  customerAcquisitionCost: number;
  churnRate: number;
  segments: CustomerSegment[];
}

export interface CustomerSegment {
  segmentName: string;
  customerCount: number;
  averageOrderValue: number;
  totalRevenue: number;
}

export interface ProductAnalytics {
  topSellingProducts: TopProduct[];
  lowPerformingProducts: TopProduct[];
  productViews: { [key: number]: number };
  conversionRates: { [key: number]: number };
}

export interface TopProduct {
  productId: number;
  productName: string;
  totalSold: number;
  totalRevenue: number;
}

export interface SubscriptionAnalytics {
  activeSubscriptions: number;
  monthlyRecurringRevenue: number;
  annualRecurringRevenue: number;
  churnRate: number;
  averageSubscriptionValue: number;
  subscriptionsByPlan: { [key: string]: number };
  growthTrend: SubscriptionGrowth[];
}

export interface SubscriptionGrowth {
  month: Date;
  newSubscriptions: number;
  cancellations: number;
  netGrowth: number;
}
