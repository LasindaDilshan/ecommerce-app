import { Injectable } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Product } from '../models/product.models';
import { ProductService } from './product.service';

@Injectable({
  providedIn: 'root'
})
export class RecentlyViewedService {
  private readonly STORAGE_KEY = 'recentlyViewedProducts';
  private readonly MAX_ITEMS = 10;
  private recentlyViewedSubject = new BehaviorSubject<Product[]>(this.loadFromStorage());

  recentlyViewed$: Observable<Product[]> = this.recentlyViewedSubject.asObservable();

  constructor(private productService: ProductService) {
    // Refresh products on service initialization to get latest data
    this.refreshProducts();
  }

  // Refresh product data from server to get latest images
  refreshProducts(): void {
    const currentProducts = this.recentlyViewedSubject.value;
    if (currentProducts.length === 0) return;

    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache'
    });

    // Fetch fresh data for each product
    const requests = currentProducts.map(p =>
      this.productService.getProductById(p.id, headers).pipe(
        catchError(() => of(p)) // Keep old data if fetch fails
      )
    );

    forkJoin(requests).subscribe(updatedProducts => {
      this.recentlyViewedSubject.next(updatedProducts);
      this.saveToStorage(updatedProducts);
    });
  }

  addProduct(product: Product): void {
    const currentProducts = this.recentlyViewedSubject.value;

    // Remove product if it already exists to avoid duplicates
    const filteredProducts = currentProducts.filter(p => p.id !== product.id);

    // Add the product to the beginning of the array
    const updatedProducts = [product, ...filteredProducts].slice(0, this.MAX_ITEMS);

    // Update the BehaviorSubject
    this.recentlyViewedSubject.next(updatedProducts);

    // Save to localStorage
    this.saveToStorage(updatedProducts);
  }

  private loadFromStorage(): Product[] {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch (error) {
      console.error('Error loading recently viewed products:', error);
      return [];
    }
  }

  private saveToStorage(products: Product[]): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(products));
    } catch (error) {
      console.error('Error saving recently viewed products:', error);
    }
  }
}
