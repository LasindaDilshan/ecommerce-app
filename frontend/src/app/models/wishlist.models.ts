export interface Wishlist {
  id: number;
  userId: number;
  items: WishlistItem[];
  itemCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface WishlistItem {
  id: number;
  productId: number;
  productName: string;
  productPrice: number;
  productDiscountPrice?: number;
  productImageUrl?: string;
  stockQuantity: number;
  isInStock: boolean;
  addedAt: string;
}

export interface AddToWishlistRequest {
  productId: number;
}

export interface MoveToCartRequest {
  wishlistItemId: number;
  quantity: number;
}
