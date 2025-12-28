import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StockItemDto {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  warehouseId: number;
  warehouseName: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  reorderPoint: number;
  reorderQuantity: number;
  lastRestockDate?: Date;
  lastCountDate?: Date;
}

export interface WarehouseDto {
  id: number;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  phone: string;
  email: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface StockMovementDto {
  id: number;
  stockItemId: number;
  productName: string;
  warehouseName: string;
  type: string;
  quantity: number;
  reference?: string;
  createdAt: Date;
  createdByUserName?: string;
}

export interface InventoryReportDto {
  totalProducts: number;
  totalStockValue: number;
  lowStockItems: number;
  outOfStockItems: number;
  stockItems: StockItemDto[];
}

export interface UpdateStockRequest {
  productId: number;
  warehouseId: number;
  quantity: number;
  reason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = `${environment.apiUrl}/inventory`;

  constructor(private http: HttpClient) {}

  // Get inventory report
  getInventoryReport(warehouseId?: number): Observable<InventoryReportDto> {
    let params = new HttpParams();
    if (warehouseId) {
      params = params.set('warehouseId', warehouseId.toString());
    }
    return this.http.get<InventoryReportDto>(`${this.apiUrl}/report`, { params });
  }

  // Get all warehouses
  getWarehouses(activeOnly: boolean = true): Observable<WarehouseDto[]> {
    const params = new HttpParams().set('activeOnly', activeOnly.toString());
    return this.http.get<WarehouseDto[]>(`${this.apiUrl}/warehouses`, { params });
  }

  // Get stock by warehouse
  getStockByWarehouse(warehouseId: number): Observable<StockItemDto[]> {
    return this.http.get<StockItemDto[]>(`${this.apiUrl}/warehouses/${warehouseId}/stock`);
  }

  // Get low stock items
  getLowStockItems(warehouseId?: number): Observable<StockItemDto[]> {
    let params = new HttpParams();
    if (warehouseId) {
      params = params.set('warehouseId', warehouseId.toString());
    }
    return this.http.get<StockItemDto[]>(`${this.apiUrl}/low-stock`, { params });
  }

  // Get stock movements
  getStockMovements(productId?: number, warehouseId?: number): Observable<StockMovementDto[]> {
    let params = new HttpParams();
    if (productId) {
      params = params.set('productId', productId.toString());
    }
    if (warehouseId) {
      params = params.set('warehouseId', warehouseId.toString());
    }
    return this.http.get<StockMovementDto[]>(`${this.apiUrl}/movements`, { params });
  }

  // Update stock
  updateStock(request: UpdateStockRequest): Observable<StockItemDto> {
    return this.http.post<StockItemDto>(`${this.apiUrl}/stock/update`, request);
  }

  // Adjust stock (add/remove)
  adjustStock(productId: number, warehouseId: number, adjustment: number, reason: string): Observable<StockItemDto> {
    return this.http.post<StockItemDto>(`${this.apiUrl}/stock/adjust`, {
      productId,
      warehouseId,
      adjustment,
      reason
    });
  }

  // Initialize stock items from existing products
  initializeFromProducts(): Observable<{ message: string; created: number; updated: number; warehouseId: number; warehouseName: string }> {
    return this.http.post<{ message: string; created: number; updated: number; warehouseId: number; warehouseName: string }>
      (`${this.apiUrl}/initialize-from-products`, {});
  }
}
