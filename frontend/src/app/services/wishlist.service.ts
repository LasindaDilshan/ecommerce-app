import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, switchMap, catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { Wishlist, WishlistItem, AddToWishlistRequest, MoveToCartRequest } from '../models/wishlist.models';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrl}/wishlist`;
  private wishlistSubject = new BehaviorSubject<Wishlist | null>(null);
  public wishlist$ = this.wishlistSubject.asObservable();

  constructor(private http: HttpClient) {}

  getWishlist(): Observable<Wishlist> {
    return this.http.get<Wishlist>(this.apiUrl).pipe(
      tap(wishlist => this.wishlistSubject.next(wishlist))
    );
  }

  addToWishlist(request: AddToWishlistRequest): Observable<Wishlist> {
    return this.http.post<Wishlist>(`${this.apiUrl}/add`, request).pipe(
      tap(wishlist => this.wishlistSubject.next(wishlist))
    );
  }

  removeFromWishlist(wishlistItemId: number): Observable<Wishlist> {
    return this.http.delete(`${this.apiUrl}/${wishlistItemId}`).pipe(
      switchMap(() => this.getWishlist()),
      catchError(error => {
        console.error('Error removing from wishlist:', error);
        throw error;
      })
    );
  }

  clearWishlist(): Observable<any> {
    return this.http.delete(this.apiUrl).pipe(
      tap(() => this.wishlistSubject.next(null))
    );
  }

  isInWishlist(productId: number): Observable<{ isInWishlist: boolean }> {
    return this.http.get<{ isInWishlist: boolean }>(`${this.apiUrl}/check/${productId}`);
  }

  moveToCart(request: MoveToCartRequest): Observable<Wishlist> {
    return this.http.post<Wishlist>(`${this.apiUrl}/move-to-cart`, request).pipe(
      tap(wishlist => this.wishlistSubject.next(wishlist))
    );
  }

  getWishlistItemCount(): number {
    const wishlist = this.wishlistSubject.value;
    return wishlist ? wishlist.itemCount : 0;
  }
}
