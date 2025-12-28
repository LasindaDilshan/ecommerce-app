import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Product } from '../models/product.models';

@Injectable({
  providedIn: 'root'
})
export class ComparisonService {
  private readonly STORAGE_KEY = 'comparisonProducts';
  private readonly MAX_ITEMS = 4;
  private comparisonSubject = new BehaviorSubject<Product[]>(this.loadFromStorage());

  comparison$: Observable<Product[]> = this.comparisonSubject.asObservable();

  constructor() {}

  addProduct(product: Product): boolean {
    const current = this.comparisonSubject.value;

    if (current.length >= this.MAX_ITEMS) {
      return false; // Max items reached
    }

    if (current.some(p => p.id === product.id)) {
      return false; // Already in comparison
    }

    const updated = [...current, product];
    this.comparisonSubject.next(updated);
    this.saveToStorage(updated);
    return true;
  }

  removeProduct(productId: number): void {
    const current = this.comparisonSubject.value;
    const updated = current.filter(p => p.id !== productId);
    this.comparisonSubject.next(updated);
    this.saveToStorage(updated);
  }

  clearAll(): void {
    this.comparisonSubject.next([]);
    localStorage.removeItem(this.STORAGE_KEY);
  }

  getProducts(): Product[] {
    return this.comparisonSubject.value;
  }

  getCount(): number {
    return this.comparisonSubject.value.length;
  }

  isInComparison(productId: number): boolean {
    return this.comparisonSubject.value.some(p => p.id === productId);
  }

  private loadFromStorage(): Product[] {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch (error) {
      console.error('Error loading comparison products:', error);
      return [];
    }
  }

  private saveToStorage(products: Product[]): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(products));
    } catch (error) {
      console.error('Error saving comparison products:', error);
    }
  }
}
