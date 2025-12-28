export interface Order {
  orderId: number;
  orderNumber: string;
  orderDate: Date;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  totalAmount: number;
  items: OrderItem[];
  shippingAddress: ShippingAddress;
}

export interface OrderItem {
  productId: number;
  productName: string;
  productImage?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreateOrderRequest {
  shippingAddress: ShippingAddress;
  paymentMethod: string;
  couponCode?: string;
}

export interface GuestCheckoutRequest {
  sessionId: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  shippingAddress: ShippingAddress;
  couponCode?: string;
}

export interface GuestOrderResponse {
  orderId: number;
  orderNumber: string;
  email: string;
  firstName: string;
  lastName: string;
  totalAmount: number;
  status: string;
  orderDate: Date;
  paymentIntentId?: string;
  clientSecret?: string;
}

export interface GuestOrderTrackingRequest {
  orderNumber: string;
  email: string;
}

export interface ShippingAddress {
  firstName: string;
  lastName: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  phone?: string;
}

export interface OrderSummary {
  orderId: number;
  orderNumber: string;
  orderDate: Date;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  totalAmount: number;
  totalItems: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4
}

export enum PaymentStatus {
  Pending = 0,
  Paid = 1,
  Failed = 2,
  Refunded = 3
}
