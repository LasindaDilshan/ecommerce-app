using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IInventoryService
    {
        // Warehouse Management
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request);
        Task<WarehouseDto> GetWarehouseByIdAsync(int warehouseId);
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync(bool activeOnly = true);
        Task<WarehouseDto> UpdateWarehouseAsync(int warehouseId, CreateWarehouseRequest request);
        Task<bool> DeleteWarehouseAsync(int warehouseId);
        Task<bool> SetDefaultWarehouseAsync(int warehouseId);

        // Stock Management
        Task<StockItemDto> GetStockItemAsync(int productId, int warehouseId);
        Task<IEnumerable<StockItemDto>> GetStockByProductAsync(int productId);
        Task<IEnumerable<StockItemDto>> GetStockByWarehouseAsync(int warehouseId);
        Task<StockItemDto> UpdateStockAsync(UpdateStockRequest request);
        Task<StockItemDto> AdjustStockAsync(StockAdjustmentRequest request);
        Task<bool> CheckStockAvailabilityAsync(int productId, int quantity, int? warehouseId = null);
        Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync(int? warehouseId = null);

        // Stock Reservation
        Task<bool> ReserveStockAsync(StockReservationRequest request);
        Task<bool> ReleaseReservationAsync(int reservationId);
        Task<bool> ConfirmReservationAsync(int reservationId);
        Task CleanupExpiredReservationsAsync();

        // Batch/Lot Management
        Task<StockBatchDto> CreateBatchAsync(CreateBatchRequest request);
        Task<IEnumerable<StockBatchDto>> GetBatchesByProductAsync(int productId, int warehouseId);
        Task<IEnumerable<StockBatchDto>> GetExpiringBatchesAsync(int daysAhead = 30);
        Task<bool> UpdateBatchAsync(int batchId, int newQuantity);

        // Stock Transfers
        Task<StockTransferDto> CreateStockTransferAsync(CreateStockTransferRequest request);
        Task<StockTransferDto> GetStockTransferAsync(int transferId);
        Task<IEnumerable<StockTransferDto>> GetStockTransfersAsync(int? warehouseId = null);
        Task<StockTransferDto> ApproveTransferAsync(int transferId, int approvedByUserId);
        Task<StockTransferDto> ShipTransferAsync(int transferId, string trackingNumber);
        Task<StockTransferDto> ReceiveTransferAsync(int transferId, Dictionary<int, int> receivedQuantities);
        Task<bool> CancelTransferAsync(int transferId);

        // Supplier Management
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request);
        Task<SupplierDto> GetSupplierByIdAsync(int supplierId);
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(bool activeOnly = true);
        Task<SupplierDto> UpdateSupplierAsync(int supplierId, CreateSupplierRequest request);
        Task<bool> DeleteSupplierAsync(int supplierId);
        Task<bool> LinkProductToSupplierAsync(int supplierId, int productId, decimal unitCost, int leadTimeDays);
        Task<IEnumerable<SupplierDto>> GetSuppliersByProductAsync(int productId);

        // Purchase Orders
        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request);
        Task<PurchaseOrderDto> GetPurchaseOrderAsync(int purchaseOrderId);
        Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(PurchaseOrderStatus? status = null);
        Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(int purchaseOrderId, int approvedByUserId);
        Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> receivedQuantities);
        Task<bool> CancelPurchaseOrderAsync(int purchaseOrderId);
        Task<IEnumerable<PurchaseOrderDto>> GenerateAutomaticPurchaseOrdersAsync();

        // Stock Movement History
        Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int? productId = null, int? warehouseId = null);
        Task RecordStockMovementAsync(int stockItemId, StockMovementType type, int quantity, string? reference = null);

        // Reports and Analytics
        Task<InventoryReportDto> GetInventoryReportAsync(int? warehouseId = null);
        Task<Dictionary<int, int>> GetStockLevelsAsync(int productId);
        Task<decimal> GetInventoryValueAsync(int? warehouseId = null);
        Task<IEnumerable<StockItemDto>> GetReorderReportAsync();
        Task<Dictionary<string, object>> GetInventoryMetricsAsync();
    }
}