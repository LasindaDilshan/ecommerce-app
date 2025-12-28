using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs
{
    // Warehouse DTOs
    public class WarehouseDto
    {
        public int WarehouseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CreateWarehouseRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    // Stock Item DTOs
    public class StockItemDto
    {
        public int StockItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public bool NeedsReorder { get; set; }
        public DateTime? LastRestockedAt { get; set; }
        public DateTime? LastSoldAt { get; set; }
    }

    public class UpdateStockRequest
    {
        [Required]
        public int StockItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public StockMovementType Type { get; set; }

        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }

    public class StockAdjustmentRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public int NewQuantity { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    // Stock Batch DTOs
    public class StockBatchDto
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string? LotNumber { get; set; }
        public int Quantity { get; set; }
        public int QuantityRemaining { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SupplierName { get; set; }
        public decimal PurchaseCost { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
    }

    public class CreateBatchRequest
    {
        [Required]
        public int StockItemId { get; set; }

        [Required]
        public string BatchNumber { get; set; } = string.Empty;

        public string? LotNumber { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? SupplierId { get; set; }

        [Required]
        public decimal PurchaseCost { get; set; }

        public string? Notes { get; set; }
    }

    // Stock Transfer DTOs
    public class StockTransferDto
    {
        public int TransferId { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public int FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; } = string.Empty;
        public int ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; } = string.Empty;
        public StockTransferStatus Status { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? TrackingNumber { get; set; }
        public List<StockTransferItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class StockTransferItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int ShippedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public string? BatchNumber { get; set; }
    }

    public class CreateStockTransferRequest
    {
        [Required]
        public int FromWarehouseId { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }

        [Required]
        public List<StockTransferItemRequest> Items { get; set; } = new();

        public string? Notes { get; set; }
    }

    public class StockTransferItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? BatchNumber { get; set; }
    }

    // Supplier DTOs
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int PaymentTermsDays { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
    }

    public class CreateSupplierRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Website { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? TaxId { get; set; }
        public int PaymentTermsDays { get; set; } = 30;
        public decimal? DiscountPercentage { get; set; }
    }

    // Purchase Order DTOs
    public class PurchaseOrderDto
    {
        public int PurchaseOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public PurchaseOrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
    }

    public class PurchaseOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CreatePurchaseOrderRequest
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public List<PurchaseOrderItemRequest> Items { get; set; } = new();

        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PurchaseOrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitCost { get; set; }
    }

    // Stock Movement/Report DTOs
    public class StockMovementDto
    {
        public int MovementId { get; set; }
        public int StockItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public StockMovementType Type { get; set; }
        public int Quantity { get; set; }
        public int BalanceBefore { get; set; }
        public int BalanceAfter { get; set; }
        public string? Reference { get; set; }
        public string? UserName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InventoryReportDto
    {
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int ExpiringItems { get; set; }
        public List<StockItemDto> LowStockProducts { get; set; } = new();
        public List<StockBatchDto> ExpiringBatches { get; set; } = new();
    }

    public class StockReservationRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public int? OrderId { get; set; }
        public int? CartItemId { get; set; }
        public int ExpirationMinutes { get; set; } = 30;
        public string? Notes { get; set; }
    }
}