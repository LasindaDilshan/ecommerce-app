import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RecentPurchase, ProductSocialProof } from '../models/social-proof.models';

@Injectable({
  providedIn: 'root'
})
export class SocialProofService {
  private apiUrl = `${environment.apiUrl}/socialproof`;

  constructor(private http: HttpClient) {}

  getRecentPurchases(limit: number = 10): Observable<RecentPurchase[]> {
    let params = new HttpParams().set('limit', limit.toString());
    return this.http.get<RecentPurchase[]>(`${this.apiUrl}/recent-purchases`, { params });
  }

  getProductSocialProof(productId: number): Observable<ProductSocialProof> {
    return this.http.get<ProductSocialProof>(`${this.apiUrl}/products/${productId}`);
  }
}
