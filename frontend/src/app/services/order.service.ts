import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Order, CreateOrderRequest, OrderSummary, PagedResult, GuestCheckoutRequest, GuestOrderResponse, GuestOrderTrackingRequest } from '../models/order.models';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;
  private guestApiUrl = `${environment.apiUrl}/guest/checkout`;

  constructor(private http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<{ order: Order; clientSecret: string }> {
    return this.http.post<{ order: Order; clientSecret: string }>(this.apiUrl, request);
  }

  getOrderById(orderId: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`);
  }

  getUserOrders(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(this.apiUrl);
  }

  getAllOrders(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Order>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Order>>(`${this.apiUrl}/all`, { params });
  }

  updateOrderStatus(orderId: number, status: number): Observable<Order> {
    return this.http.put<Order>(`${this.apiUrl}/${orderId}/status`, { status });
  }

  confirmPayment(orderId: number, paymentIntentId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${orderId}/confirm-payment`, { paymentIntentId });
  }

  // Guest checkout methods
  createGuestOrder(request: GuestCheckoutRequest): Observable<GuestOrderResponse> {
    return this.http.post<GuestOrderResponse>(this.guestApiUrl, request);
  }

  trackGuestOrder(request: GuestOrderTrackingRequest): Observable<GuestOrderResponse> {
    const params = new HttpParams()
      .set('orderNumber', request.orderNumber)
      .set('email', request.email);

    return this.http.get<GuestOrderResponse>(`${this.guestApiUrl}/track`, { params });
  }

  cancelOrder(orderId: number): Observable<Order> {
    // Use updateOrderStatus with Cancelled status (4)
    return this.updateOrderStatus(orderId, 4);
  }
}
