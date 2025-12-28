// Enums
export enum StockMovementType {
  Received = 'Received',
  Sold = 'Sold',
  Adjusted = 'Adjusted',
  Transferred = 'Transferred',
  Returned = 'Returned',
  Damaged = 'Damaged',
  Reserved = 'Reserved',
  Released = 'Released'
}

export enum StockTransferStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Shipped = 'Shipped',
  Received = 'Received',
  Cancelled = 'Cancelled'
}

export enum PurchaseOrderStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  Ordered = 'Ordered',
  PartiallyReceived = 'PartiallyReceived',
  Received = 'Received',
  Cancelled = 'Cancelled'
}

// Warehouse DTOs
export interface Warehouse {
  warehouseId: number;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  phone: string;
  email?: string;
  isActive: boolean;
  isDefault: boolean;
  latitude: number;
  longitude: number;
  totalProducts: number;
  totalStock: number;
  totalValue: number;
}

export interface CreateWarehouseRequest {
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  phone: string;
  email?: string;
  latitude?: number;
  longitude?: number;
  isDefault?: boolean;
  isActive?: boolean;
}

// Stock Item DTOs
export interface StockItem {
  stockItemId: number;
  productId: number;
  productName: string;
  productSKU: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  reorderPoint: number;
  reorderQuantity: number;
  unitCost: number;
  totalValue: number;
  needsReorder: boolean;
  lastRestockedAt?: Date;
  lastSoldAt?: Date;
}

export interface UpdateStockRequest {
  stockItemId: number;
  quantity: number;
  type: StockMovementType;
  reference?: string;
  notes?: string;
}

export interface StockAdjustmentRequest {
  productId: number;
  warehouseId: number;
  newQuantity: number;
  reason: string;
  notes?: string;
}

// Stock Batch DTOs
export interface StockBatch {
  batchId: number;
  batchNumber: string;
  lotNumber?: string;
  quantity: number;
  quantityRemaining: number;
  manufactureDate?: Date;
  expiryDate?: Date;
  supplierName?: string;
  purchaseCost: number;
  receivedDate: Date;
  isExpired: boolean;
  isExpiringSoon: boolean;
}

export interface CreateBatchRequest {
  stockItemId: number;
  batchNumber: string;
  lotNumber?: string;
  quantity: number;
  manufactureDate?: Date;
  expiryDate?: Date;
  supplierId?: number;
  purchaseCost: number;
  notes?: string;
}

// Stock Transfer DTOs
export interface StockTransfer {
  transferId: number;
  transferNumber: string;
  fromWarehouseId: number;
  fromWarehouseName: string;
  toWarehouseId: number;
  toWarehouseName: string;
  status: StockTransferStatus;
  requestedDate: Date;
  shippedDate?: Date;
  receivedDate?: Date;
  trackingNumber?: string;
  items: StockTransferItem[];
  totalItems: number;
  totalQuantity: number;
}

export interface StockTransferItem {
  productId: number;
  productName: string;
  productSKU: string;
  requestedQuantity: number;
  shippedQuantity: number;
  receivedQuantity: number;
  batchNumber?: string;
}

export interface CreateStockTransferRequest {
  fromWarehouseId: number;
  toWarehouseId: number;
  items: StockTransferItemRequest[];
  notes?: string;
}

export interface StockTransferItemRequest {
  productId: number;
  quantity: number;
  batchNumber?: string;
}

// Supplier DTOs
export interface Supplier {
  supplierId: number;
  name: string;
  code: string;
  contactPerson: string;
  email: string;
  phone: string;
  website?: string;
  address: string;
  city: string;
  state: string;
  country: string;
  paymentTermsDays: number;
  discountPercentage?: number;
  isActive: boolean;
  totalProducts: number;
  totalOrders: number;
}

export interface CreateSupplierRequest {
  name: string;
  code: string;
  contactPerson: string;
  email: string;
  phone: string;
  website?: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  taxId?: string;
  paymentTermsDays?: number;
  discountPercentage?: number;
}

// Purchase Order DTOs
export interface PurchaseOrder {
  purchaseOrderId: number;
  orderNumber: string;
  supplierId: number;
  supplierName: string;
  warehouseId: number;
  warehouseName: string;
  status: PurchaseOrderStatus;
  subTotal: number;
  taxAmount: number;
  shippingCost: number;
  totalAmount: number;
  orderDate: Date;
  expectedDeliveryDate?: Date;
  receivedDate?: Date;
  items: PurchaseOrderItem[];
}

export interface PurchaseOrderItem {
  productId: number;
  productName: string;
  productSKU: string;
  orderedQuantity: number;
  receivedQuantity: number;
  unitCost: number;
  totalCost: number;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  warehouseId: number;
  items: PurchaseOrderItemRequest[];
  expectedDeliveryDate?: Date;
  notes?: string;
}

export interface PurchaseOrderItemRequest {
  productId: number;
  quantity: number;
  unitCost: number;
}

// Stock Movement/Report DTOs
export interface StockMovement {
  movementId: number;
  stockItemId: number;
  productName: string;
  warehouseName: string;
  type: StockMovementType;
  quantity: number;
  balanceBefore: number;
  balanceAfter: number;
  reference?: string;
  userName?: string;
  notes?: string;
  createdAt: Date;
}

export interface InventoryReport {
  totalProducts: number;
  totalStock: number;
  totalValue: number;
  lowStockItems: number;
  outOfStockItems: number;
  expiringItems: number;
  lowStockProducts: StockItem[];
  expiringBatches: StockBatch[];
}

export interface StockReservationRequest {
  productId: number;
  warehouseId: number;
  quantity: number;
  orderId?: number;
  cartItemId?: number;
  expirationMinutes?: number;
  notes?: string;
}
