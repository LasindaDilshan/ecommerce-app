import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  SubscriptionPlan,
  CreateSubscriptionPlanRequest,
  Subscription,
  CreateSubscriptionRequest,
  UpdateSubscriptionRequest,
  PauseSubscriptionRequest,
  CancelSubscriptionRequest,
  SubscriptionPayment,
  SubscriptionOrder,
  ReturnRequest,
  CreateReturnRequest,
  ProcessReturnRequest,
  ReturnStatus,
  AbandonedCart,
  RecoverAbandonedCartRequest,
  GiftCard,
  CreateGiftCardRequest,
  RedeemGiftCardRequest,
  GiftCardTransaction,
  SalesAnalytics,
  CustomerAnalytics,
  ProductAnalytics,
  SubscriptionAnalytics
} from '../models/subscription.models';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private apiUrl = `${environment.apiUrl}/subscription`;

  constructor(private http: HttpClient) {}

  // ==================== Subscription Plans ====================

  getPlans(activeOnly: boolean = true): Observable<SubscriptionPlan[]> {
    const params = new HttpParams().set('activeOnly', activeOnly.toString());
    return this.http.get<SubscriptionPlan[]>(`${this.apiUrl}/plans`, { params });
  }

  getPlanById(id: number): Observable<SubscriptionPlan> {
    return this.http.get<SubscriptionPlan>(`${this.apiUrl}/plans/${id}`);
  }

  createPlan(request: CreateSubscriptionPlanRequest): Observable<SubscriptionPlan> {
    return this.http.post<SubscriptionPlan>(`${this.apiUrl}/plans`, request);
  }

  updatePlan(id: number, request: CreateSubscriptionPlanRequest): Observable<SubscriptionPlan> {
    return this.http.put<SubscriptionPlan>(`${this.apiUrl}/plans/${id}`, request);
  }

  deletePlan(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/plans/${id}`);
  }

  togglePlanStatus(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/plans/${id}/toggle-status`, {});
  }

  // ==================== Subscriptions ====================

  getSubscription(id: number): Observable<Subscription> {
    return this.http.get<Subscription>(`${this.apiUrl}/${id}`);
  }

  getUserSubscriptions(userId: number): Observable<Subscription[]> {
    return this.http.get<Subscription[]>(`${this.apiUrl}/user/${userId}`);
  }

  createSubscription(request: CreateSubscriptionRequest): Observable<Subscription> {
    return this.http.post<Subscription>(this.apiUrl, request);
  }

  updateSubscription(id: number, request: UpdateSubscriptionRequest): Observable<Subscription> {
    return this.http.put<Subscription>(`${this.apiUrl}/${id}`, request);
  }

  pauseSubscription(id: number, request: PauseSubscriptionRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/pause`, request);
  }

  resumeSubscription(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/resume`, {});
  }

  cancelSubscription(id: number, request: CancelSubscriptionRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, request);
  }

  reactivateSubscription(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/reactivate`, {});
  }

  changePlan(id: number, newPlanId: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/${id}/change-plan`, newPlanId);
  }

  calculateProration(id: number, newPlanId: number): Observable<number> {
    const params = new HttpParams().set('newPlanId', newPlanId.toString());
    return this.http.get<number>(`${this.apiUrl}/${id}/calculate-proration`, { params });
  }

  // ==================== Subscription Billing ====================

  processPayment(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/process-payment`, {});
  }

  getPayments(id: number): Observable<SubscriptionPayment[]> {
    return this.http.get<SubscriptionPayment[]>(`${this.apiUrl}/${id}/payments`);
  }

  retryPayment(paymentId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/payments/${paymentId}/retry`, {});
  }

  // ==================== Subscription Orders ====================

  createOrder(id: number): Observable<SubscriptionOrder> {
    return this.http.post<SubscriptionOrder>(`${this.apiUrl}/${id}/create-order`, {});
  }

  skipNextOrder(id: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/skip-next-order`, JSON.stringify(reason), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  getUpcomingOrders(id: number): Observable<SubscriptionOrder[]> {
    return this.http.get<SubscriptionOrder[]>(`${this.apiUrl}/${id}/upcoming-orders`);
  }

  updateShippingAddress(id: number, addressId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/shipping-address`, addressId);
  }

  // ==================== Trial Management ====================

  startTrial(planId: number): Observable<void> {
    const params = new HttpParams().set('planId', planId.toString());
    return this.http.post<void>(`${this.apiUrl}/start-trial`, {}, { params });
  }

  endTrial(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/end-trial`, {});
  }

  extendTrial(id: number, days: number): Observable<void> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.post<void>(`${this.apiUrl}/${id}/extend-trial`, {}, { params });
  }

  // ==================== Gift Subscriptions ====================

  createGiftSubscription(recipientUserId: number, planId: number, message?: string): Observable<Subscription> {
    const params = new HttpParams()
      .set('recipientUserId', recipientUserId.toString())
      .set('planId', planId.toString());
    return this.http.post<Subscription>(`${this.apiUrl}/gift`, message ? JSON.stringify(message) : null, {
      params,
      headers: { 'Content-Type': 'application/json' }
    });
  }

  redeemGiftSubscription(redemptionCode: string): Observable<void> {
    const params = new HttpParams().set('redemptionCode', redemptionCode);
    return this.http.post<void>(`${this.apiUrl}/gift/redeem`, {}, { params });
  }

  // ==================== Returns & RMA ====================

  createReturn(request: CreateReturnRequest): Observable<ReturnRequest> {
    return this.http.post<ReturnRequest>(`${this.apiUrl}/returns`, request);
  }

  getReturn(id: number): Observable<ReturnRequest> {
    return this.http.get<ReturnRequest>(`${this.apiUrl}/returns/${id}`);
  }

  getUserReturns(userId: number): Observable<ReturnRequest[]> {
    return this.http.get<ReturnRequest[]>(`${this.apiUrl}/returns/user/${userId}`);
  }

  getAllReturns(status?: ReturnStatus): Observable<ReturnRequest[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<ReturnRequest[]>(`${this.apiUrl}/returns`, { params });
  }

  processReturn(id: number, request: ProcessReturnRequest): Observable<ReturnRequest> {
    return this.http.post<ReturnRequest>(`${this.apiUrl}/returns/${id}/process`, request);
  }

  updateReturnTracking(id: number, trackingNumber: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/returns/${id}/tracking`, JSON.stringify(trackingNumber), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  completeReturn(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/returns/${id}/complete`, {});
  }

  // ==================== Abandoned Cart Recovery ====================

  detectAbandonedCart(userId?: number, sessionId?: string): Observable<AbandonedCart> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    if (sessionId) params = params.set('sessionId', sessionId);
    return this.http.post<AbandonedCart>(`${this.apiUrl}/abandoned-cart/detect`, {}, { params });
  }

  getAbandonedCarts(since?: Date): Observable<AbandonedCart[]> {
    let params = new HttpParams();
    if (since) params = params.set('since', since.toISOString());
    return this.http.get<AbandonedCart[]>(`${this.apiUrl}/abandoned-carts`, { params });
  }

  sendRecoveryEmail(request: RecoverAbandonedCartRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/abandoned-cart/send-recovery`, request);
  }

  recoverCart(recoveryCode: string): Observable<void> {
    const params = new HttpParams().set('recoveryCode', recoveryCode);
    return this.http.post<void>(`${this.apiUrl}/abandoned-cart/recover`, {}, { params });
  }

  markCartAsRecovered(id: number, orderId: number): Observable<void> {
    const params = new HttpParams().set('orderId', orderId.toString());
    return this.http.post<void>(`${this.apiUrl}/abandoned-cart/${id}/mark-recovered`, {}, { params });
  }

  getRecoveryRate(startDate: Date, endDate: Date): Observable<number> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    return this.http.get<number>(`${this.apiUrl}/abandoned-cart/recovery-rate`, { params });
  }

  // ==================== Gift Cards ====================

  createGiftCard(request: CreateGiftCardRequest): Observable<GiftCard> {
    return this.http.post<GiftCard>(`${this.apiUrl}/gift-cards`, request);
  }

  getGiftCard(code: string): Observable<GiftCard> {
    return this.http.get<GiftCard>(`${this.apiUrl}/gift-cards/${code}`);
  }

  getGiftCardBalance(code: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/gift-cards/${code}/balance`);
  }

  redeemGiftCard(request: RedeemGiftCardRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/gift-cards/redeem`, request);
  }

  reloadGiftCard(code: string, amount: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/gift-cards/${code}/reload`, amount);
  }

  getUserGiftCards(userId: number): Observable<GiftCard[]> {
    return this.http.get<GiftCard[]>(`${this.apiUrl}/gift-cards/user/${userId}`);
  }

  getGiftCardTransactions(code: string): Observable<GiftCardTransaction[]> {
    return this.http.get<GiftCardTransaction[]>(`${this.apiUrl}/gift-cards/${code}/transactions`);
  }

  deactivateGiftCard(code: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/gift-cards/${code}/deactivate`, {});
  }

  // ==================== Analytics ====================

  getSalesAnalytics(startDate: Date, endDate: Date): Observable<SalesAnalytics> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    return this.http.get<SalesAnalytics>(`${this.apiUrl}/analytics/sales`, { params });
  }

  getCustomerAnalytics(startDate: Date, endDate: Date): Observable<CustomerAnalytics> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    return this.http.get<CustomerAnalytics>(`${this.apiUrl}/analytics/customers`, { params });
  }

  getProductAnalytics(startDate: Date, endDate: Date, categoryId?: number): Observable<ProductAnalytics> {
    let params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    if (categoryId) params = params.set('categoryId', categoryId.toString());
    return this.http.get<ProductAnalytics>(`${this.apiUrl}/analytics/products`, { params });
  }

  getSubscriptionAnalytics(): Observable<SubscriptionAnalytics> {
    return this.http.get<SubscriptionAnalytics>(`${this.apiUrl}/analytics/subscriptions`);
  }

  getMonthlyRecurringRevenue(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/analytics/mrr`);
  }

  getChurnRate(startDate: Date, endDate: Date): Observable<number> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    return this.http.get<number>(`${this.apiUrl}/analytics/churn-rate`, { params });
  }

  getCustomerLifetimeValue(userId?: number): Observable<number> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<number>(`${this.apiUrl}/analytics/customer-lifetime-value`, { params });
  }

  // ==================== Batch Operations ====================

  processDueSubscriptions(): Observable<{ ProcessedCount: number }> {
    return this.http.post<{ ProcessedCount: number }>(`${this.apiUrl}/batch/process-due-subscriptions`, {});
  }

  sendPaymentReminders(): Observable<{ SentCount: number }> {
    return this.http.post<{ SentCount: number }>(`${this.apiUrl}/batch/send-payment-reminders`, {});
  }

  processAbandonedCarts(): Observable<{ ProcessedCount: number }> {
    return this.http.post<{ ProcessedCount: number }>(`${this.apiUrl}/batch/process-abandoned-carts`, {});
  }

  expireGiftCards(): Observable<{ ExpiredCount: number }> {
    return this.http.post<{ ExpiredCount: number }>(`${this.apiUrl}/batch/expire-gift-cards`, {});
  }
}
