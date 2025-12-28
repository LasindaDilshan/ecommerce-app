export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  stockQuantity: number;
  sku: string;
  imageUrl?: string;
  isActive: boolean;
  isFeatured: boolean;
  categoryId: number;
  categoryName: string;
  additionalImages?: string[];
  rating?: number;
  reviewCount?: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  stockQuantity: number;
  sku: string;
  imageUrl?: string;
  categoryId: number;
  isFeatured: boolean;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  stockQuantity: number;
  imageUrl?: string;
  categoryId: number;
  isActive: boolean;
  isFeatured: boolean;
}

export interface ProductQueryParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
