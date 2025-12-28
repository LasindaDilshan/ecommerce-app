export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  isActive: boolean;
  parentCategoryId?: number;
  productCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  description: string;
  parentCategoryId?: number;
}

export interface UpdateCategoryRequest {
  name: string;
  description: string;
  isActive: boolean;
  parentCategoryId?: number;
}
