import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
  ProductQueryParams,
  PagedResult
} from '../models/product.models';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getProducts(params: ProductQueryParams, headers?: HttpHeaders): Observable<PagedResult<Product>> {
    let httpParams = new HttpParams();

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId.toString());
    if (params.minPrice) httpParams = httpParams.set('minPrice', params.minPrice.toString());
    if (params.maxPrice) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
    if (params.isFeatured !== undefined) httpParams = httpParams.set('isFeatured', params.isFeatured.toString());
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortOrder) httpParams = httpParams.set('sortOrder', params.sortOrder);

    const options: { params: HttpParams; headers?: HttpHeaders } = { params: httpParams };
    if (headers) {
      options.headers = headers;
    }

    return this.http.get<PagedResult<Product>>(this.apiUrl, options);
  }

  getFeaturedProducts(headers?: HttpHeaders): Observable<Product[]> {
    const options: { headers?: HttpHeaders } = {};
    if (headers) {
      options.headers = headers;
    }
    return this.http.get<Product[]>(`${this.apiUrl}/featured`, options);
  }

  getProductById(id: number, headers?: HttpHeaders): Observable<Product> {
    const options: { headers?: HttpHeaders } = {};
    if (headers) {
      options.headers = headers;
    }
    return this.http.get<Product>(`${this.apiUrl}/${id}`, options);
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  updateProduct(id: number, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  uploadProductImage(id: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${id}/upload-image`, formData);
  }

  getSimilarProducts(id: number, limit: number = 4): Observable<Product[]> {
    let httpParams = new HttpParams().set('limit', limit.toString());
    return this.http.get<Product[]>(`${this.apiUrl}/${id}/similar`, { params: httpParams });
  }

  getCustomersAlsoBought(id: number, limit: number = 4): Observable<Product[]> {
    let httpParams = new HttpParams().set('limit', limit.toString());
    return this.http.get<Product[]>(`${this.apiUrl}/${id}/customers-also-bought`, { params: httpParams });
  }

  getPersonalizedRecommendations(limit: number = 8): Observable<Product[]> {
    let httpParams = new HttpParams().set('limit', limit.toString());
    return this.http.get<Product[]>(`${this.apiUrl}/recommendations`, { params: httpParams });
  }
}
