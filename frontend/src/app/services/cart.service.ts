import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Cart, AddToCartRequest, UpdateCartItemRequest } from '../models/cart.models';
import { GuestSessionService } from './guest-session.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;
  private guestApiUrl = `${environment.apiUrl}/guest/cart`;
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  public cart$ = this.cartSubject.asObservable();

  constructor(
    private http: HttpClient,
    private guestSessionService: GuestSessionService,
    private authService: AuthService
  ) {}

  // Auto-detect auth state and call appropriate method
  getCart(): Observable<Cart> {
    if (this.authService.isLoggedIn()) {
      return this.getUserCart();
    } else {
      return this.getGuestCart();
    }
  }

  addToCart(request: AddToCartRequest): Observable<Cart> {
    if (this.authService.isLoggedIn()) {
      return this.addToUserCart(request);
    } else {
      return this.addToGuestCart(request);
    }
  }

  updateCartItem(cartItemId: number, request: UpdateCartItemRequest): Observable<Cart> {
    if (this.authService.isLoggedIn()) {
      return this.updateUserCartItem(cartItemId, request);
    } else {
      return this.updateGuestCartItem(cartItemId, request);
    }
  }

  removeFromCart(cartItemId: number): Observable<any> {
    if (this.authService.isLoggedIn()) {
      return this.removeFromUserCart(cartItemId);
    } else {
      return this.removeFromGuestCart(cartItemId);
    }
  }

  clearCart(): Observable<any> {
    if (this.authService.isLoggedIn()) {
      return this.clearUserCart();
    } else {
      return this.clearGuestCart();
    }
  }

  getCartItemCount(): number {
    return this.cartSubject.value?.totalItems || 0;
  }

  // User cart methods
  private getUserCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private addToUserCart(request: AddToCartRequest): Observable<Cart> {
    return this.http.post<Cart>(`${this.apiUrl}/add`, request).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private updateUserCartItem(cartItemId: number, request: UpdateCartItemRequest): Observable<Cart> {
    return this.http.put<Cart>(`${this.apiUrl}/${cartItemId}`, request).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private removeFromUserCart(cartItemId: number): Observable<Cart> {
    return this.http.delete(`${this.apiUrl}/${cartItemId}`).pipe(
      switchMap(() => this.getUserCart())
    );
  }

  private clearUserCart(): Observable<any> {
    return this.http.delete(this.apiUrl).pipe(
      tap(() => this.cartSubject.next(null))
    );
  }

  // Guest cart methods
  private getGuestCart(): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.get<Cart>(`${this.guestApiUrl}?sessionId=${sessionId}`).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private addToGuestCart(request: AddToCartRequest): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    const guestRequest = {
      sessionId,
      productId: request.productId,
      quantity: request.quantity
    };
    return this.http.post<Cart>(`${this.guestApiUrl}/add`, guestRequest).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private updateGuestCartItem(cartItemId: number, request: UpdateCartItemRequest): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.put<Cart>(`${this.guestApiUrl}/${cartItemId}?sessionId=${sessionId}`, request).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private removeFromGuestCart(cartItemId: number): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.delete(`${this.guestApiUrl}/${cartItemId}?sessionId=${sessionId}`).pipe(
      switchMap(() => this.getGuestCart())
    );
  }

  private clearGuestCart(): Observable<any> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.delete(`${this.guestApiUrl}?sessionId=${sessionId}`).pipe(
      tap(() => this.cartSubject.next(null))
    );
  }

  // Cart merging
  mergeGuestCart(): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.post<Cart>(`${this.guestApiUrl}/merge`, { sessionId }).pipe(
      tap(cart => {
        this.cartSubject.next(cart);
        this.guestSessionService.clearSession();
      })
    );
  }

  // Coupon operations
  applyCoupon(couponCode: string): Observable<Cart> {
    if (this.authService.isLoggedIn()) {
      return this.applyUserCoupon(couponCode);
    } else {
      return this.applyGuestCoupon(couponCode);
    }
  }

  private applyUserCoupon(couponCode: string): Observable<Cart> {
    return this.http.post<Cart>(`${this.apiUrl}/apply-coupon`, { couponCode }).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private applyGuestCoupon(couponCode: string): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.post<Cart>(`${this.guestApiUrl}/apply-coupon`, {
      couponCode,
      sessionId
    }).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  removeCoupon(): Observable<Cart> {
    if (this.authService.isLoggedIn()) {
      return this.removeUserCoupon();
    } else {
      return this.removeGuestCoupon();
    }
  }

  private removeUserCoupon(): Observable<Cart> {
    return this.http.delete<Cart>(`${this.apiUrl}/remove-coupon`).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  private removeGuestCoupon(): Observable<Cart> {
    const sessionId = this.guestSessionService.getSessionId();
    return this.http.delete<Cart>(`${this.guestApiUrl}/remove-coupon?sessionId=${sessionId}`).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }
}
