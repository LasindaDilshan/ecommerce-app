import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SearchResult {
  products: SearchProduct[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  facets?: SearchFacets;
}

export interface SearchProduct {
  id: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  imageUrl: string;
  categoryName: string;
  rating: number;
  reviewCount: number;
  stockQuantity: number;
  highlightedName?: string;
  highlightedDescription?: string;
}

export interface SearchFacets {
  categories: FacetItem[];
  priceRanges: FacetItem[];
}

export interface FacetItem {
  label: string;
  value: string;
  count: number;
}

export interface AutocompleteResult {
  products: { id: number; name: string; imageUrl: string; price: number }[];
  categories: { id: number; name: string }[];
  suggestions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private apiUrl = `${environment.apiUrl}/search`;

  constructor(private http: HttpClient) {}

  search(query: string, filters?: {
    categoryId?: number;
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    onSale?: boolean;
    sortBy?: string;
    page?: number;
    pageSize?: number;
  }): Observable<SearchResult> {
    let params = new HttpParams().set('query', query);

    if (filters) {
      if (filters.categoryId) params = params.set('categoryId', filters.categoryId.toString());
      if (filters.minPrice !== undefined) params = params.set('minPrice', filters.minPrice.toString());
      if (filters.maxPrice !== undefined) params = params.set('maxPrice', filters.maxPrice.toString());
      if (filters.inStock !== undefined) params = params.set('inStock', filters.inStock.toString());
      if (filters.onSale !== undefined) params = params.set('onSale', filters.onSale.toString());
      if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
      if (filters.page) params = params.set('page', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    }

    return this.http.get<SearchResult>(`${this.apiUrl}/products`, { params });
  }

  autocomplete(query: string): Observable<AutocompleteResult> {
    const params = new HttpParams().set('query', query);
    return this.http.get<AutocompleteResult>(`${this.apiUrl}/autocomplete`, { params });
  }

  getPopularSearches(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/popular`);
  }

  getSearchHistory(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/history`);
  }
}
