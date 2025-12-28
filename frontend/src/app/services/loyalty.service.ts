import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  LoyaltyAccount,
  LoyaltyTransaction,
  LoyaltyReward,
  RedeemedReward,
  LoyaltySummary,
  RedeemRewardResponse,
  CreateRewardRequest,
  AdjustPointsRequest
} from '../models/loyalty.models';

@Injectable({
  providedIn: 'root'
})
export class LoyaltyService {
  private apiUrl = `${environment.apiUrl}/loyalty`;

  constructor(private http: HttpClient) {}

  // User endpoints
  getAccount(): Observable<LoyaltyAccount> {
    return this.http.get<LoyaltyAccount>(`${this.apiUrl}/account`);
  }

  getSummary(): Observable<LoyaltySummary> {
    return this.http.get<LoyaltySummary>(`${this.apiUrl}/summary`);
  }

  getTransactions(page: number = 1, pageSize: number = 20): Observable<LoyaltyTransaction[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<LoyaltyTransaction[]>(`${this.apiUrl}/transactions`, { params });
  }

  getAvailableRewards(): Observable<LoyaltyReward[]> {
    return this.http.get<LoyaltyReward[]>(`${this.apiUrl}/rewards`);
  }

  redeemReward(rewardId: number): Observable<RedeemRewardResponse> {
    return this.http.post<RedeemRewardResponse>(`${this.apiUrl}/rewards/${rewardId}/redeem`, {});
  }

  getRedeemedRewards(): Observable<RedeemedReward[]> {
    return this.http.get<RedeemedReward[]>(`${this.apiUrl}/redeemed`);
  }

  validateRedemptionCode(code: string): Observable<RedeemedReward> {
    return this.http.get<RedeemedReward>(`${this.apiUrl}/validate/${code}`);
  }

  // Admin endpoints
  getAllRewards(): Observable<LoyaltyReward[]> {
    return this.http.get<LoyaltyReward[]>(`${this.apiUrl}/admin/rewards`);
  }

  createReward(request: CreateRewardRequest): Observable<LoyaltyReward> {
    return this.http.post<LoyaltyReward>(`${this.apiUrl}/admin/rewards`, request);
  }

  updateReward(rewardId: number, request: CreateRewardRequest): Observable<LoyaltyReward> {
    return this.http.put<LoyaltyReward>(`${this.apiUrl}/admin/rewards/${rewardId}`, request);
  }

  deleteReward(rewardId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/admin/rewards/${rewardId}`);
  }

  adjustPoints(request: AdjustPointsRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/admin/adjust`, request);
  }
}
