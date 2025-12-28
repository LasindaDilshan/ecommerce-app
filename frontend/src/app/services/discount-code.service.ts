import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  DiscountCode,
  CreateDiscountCodeRequest,
  UpdateDiscountCodeRequest,
  ApplyCouponRequest,
  CouponValidationResponse,
  DiscountCodeStats
} from '../models/discount-code.models';

@Injectable({
  providedIn: 'root'
})
export class DiscountCodeService {
  private apiUrl = `${environment.apiUrl}/discount-codes`;

  constructor(private http: HttpClient) {}

  // Admin Methods (require Admin role)

  /**
   * Gets all discount codes
   */
  getAllDiscountCodes(activeOnly: boolean = false): Observable<DiscountCode[]> {
    return this.http.get<DiscountCode[]>(`${this.apiUrl}?activeOnly=${activeOnly}`);
  }

  /**
   * Gets a discount code by ID
   */
  getDiscountCodeById(id: number): Observable<DiscountCode> {
    return this.http.get<DiscountCode>(`${this.apiUrl}/${id}`);
  }

  /**
   * Gets a discount code by code
   */
  getDiscountCodeByCode(code: string): Observable<DiscountCode> {
    return this.http.get<DiscountCode>(`${this.apiUrl}/by-code/${code}`);
  }

  /**
   * Creates a new discount code
   */
  createDiscountCode(request: CreateDiscountCodeRequest): Observable<DiscountCode> {
    return this.http.post<DiscountCode>(this.apiUrl, request);
  }

  /**
   * Updates an existing discount code
   */
  updateDiscountCode(id: number, request: UpdateDiscountCodeRequest): Observable<DiscountCode> {
    return this.http.put<DiscountCode>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Deletes a discount code
   */
  deleteDiscountCode(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Gets discount code statistics
   */
  getStats(): Observable<DiscountCodeStats> {
    return this.http.get<DiscountCodeStats>(`${this.apiUrl}/stats`);
  }

  /**
   * Gets discount type display name
   */
  getDiscountTypeDisplay(discountType: number): string {
    switch (discountType) {
      case 1:
        return 'Percentage';
      case 2:
        return 'Fixed Amount';
      case 3:
        return 'Free Shipping';
      case 4:
        return 'Buy X Get Y';
      default:
        return 'Unknown';
    }
  }

  /**
   * Formats discount value for display
   */
  formatDiscountValue(discountType: number, value: number): string {
    switch (discountType) {
      case 1: // Percentage
        return `${value}% off`;
      case 2: // Fixed Amount
        return `$${value.toFixed(2)} off`;
      case 3: // Free Shipping
        return 'Free Shipping';
      case 4: // Buy X Get Y
        return 'Special Offer';
      default:
        return '';
    }
  }
}
