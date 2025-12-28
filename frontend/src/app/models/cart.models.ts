export interface Cart {
  cartId: number;
  items: CartItem[];
  subTotal: number;
  discountAmount: number;
  couponCode?: string;
  finalTotal: number;
  totalItems: number;
}

export interface CartItem {
  cartItemId: number;
  productId: number;
  productName: string;
  productImage?: string;
  price: number;
  discountPrice?: number;
  quantity: number;
  totalPrice: number;
  availableStock: number;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}
