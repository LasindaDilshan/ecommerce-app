using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    // Warehouse/Location Management
    public class Warehouse
    {
        [Key]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
        public ICollection<StockTransfer> OutgoingTransfers { get; set; } = new List<StockTransfer>();
        public ICollection<StockTransfer> IncomingTransfers { get; set; } = new List<StockTransfer>();
    }

    // Stock/Inventory Item
    public class StockItem
    {
        [Key]
        public int StockItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Required]
        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!;

        public int QuantityOnHand { get; set; } = 0;

        public int QuantityReserved { get; set; } = 0;

        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        public int ReorderPoint { get; set; } = 10;

        public int ReorderQuantity { get; set; } = 50;

        public int MinimumStock { get; set; } = 5;

        public int MaximumStock { get; set; } = 1000;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        public DateTime? LastRestockedAt { get; set; }

        public DateTime? LastSoldAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<StockBatch> Batches { get; set; } = new List<StockBatch>();
        public ICollection<StockReservation> Reservations { get; set; } = new List<StockReservation>();
    }

    // Batch/Lot Tracking
    public class StockBatch
    {
        [Key]
        public int BatchId { get; set; }

        [Required]
        public int StockItemId { get; set; }

        [ForeignKey("StockItemId")]
        public StockItem StockItem { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LotNumber { get; set; }

        public int Quantity { get; set; }

        public int QuantityRemaining { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchaseCost { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }
    }

    // Stock Reservation (for pending orders)
    public class StockReservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int StockItemId { get; set; }

        [ForeignKey("StockItemId")]
        public StockItem StockItem { get; set; } = null!;

        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        public int? CartItemId { get; set; }

        public int Quantity { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }
    }

    // Stock Transfer between warehouses
    public class StockTransfer
    {
        [Key]
        public int TransferId { get; set; }

        [Required]
        [StringLength(50)]
        public string TransferNumber { get; set; } = string.Empty;

        [Required]
        public int FromWarehouseId { get; set; }

        [ForeignKey("FromWarehouseId")]
        public Warehouse FromWarehouse { get; set; } = null!;

        [Required]
        public int ToWarehouseId { get; set; }

        [ForeignKey("ToWarehouseId")]
        public Warehouse ToWarehouse { get; set; } = null!;

        public StockTransferStatus Status { get; set; } = StockTransferStatus.Pending;

        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public int? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<StockTransferItem> TransferItems { get; set; } = new List<StockTransferItem>();
    }

    // Stock Transfer Line Items
    public class StockTransferItem
    {
        [Key]
        public int TransferItemId { get; set; }

        [Required]
        public int TransferId { get; set; }

        [ForeignKey("TransferId")]
        public StockTransfer Transfer { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int RequestedQuantity { get; set; }

        public int ShippedQuantity { get; set; }

        public int ReceivedQuantity { get; set; }

        public string? BatchNumber { get; set; }

        public string? Notes { get; set; }
    }

    // Stock Movement/Transaction Log
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        [Required]
        public int StockItemId { get; set; }

        [ForeignKey("StockItemId")]
        public StockItem StockItem { get; set; } = null!;

        public StockMovementType Type { get; set; }

        public int Quantity { get; set; }

        public int BalanceBefore { get; set; }

        public int BalanceAfter { get; set; }

        public int? OrderId { get; set; }

        public int? TransferId { get; set; }

        public int? PurchaseOrderId { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string? Reference { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Supplier Management
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public string? Website { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TaxId { get; set; }

        public int PaymentTermsDays { get; set; } = 30;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }

    // Supplier Product Mapping (which products a supplier provides)
    public class SupplierProduct
    {
        [Key]
        public int SupplierProductId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [StringLength(50)]
        public string? SupplierSKU { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        public int MinimumOrderQuantity { get; set; } = 1;

        public int LeadTimeDays { get; set; } = 7;

        public bool IsPreferred { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    // Purchase Order
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!;

        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public int? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    }

    // Purchase Order Line Items
    public class PurchaseOrderItem
    {
        [Key]
        public int PurchaseOrderItemId { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int OrderedQuantity { get; set; }

        public int ReceivedQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost => OrderedQuantity * UnitCost;

        public string? Notes { get; set; }
    }

    // Enums
    public enum StockMovementType
    {
        Purchase,
        Sale,
        Return,
        Adjustment,
        Transfer,
        Reservation,
        WriteOff,
        Recount
    }

    public enum StockTransferStatus
    {
        Pending,
        Approved,
        Rejected,
        InTransit,
        Received,
        Cancelled
    }

    public enum PurchaseOrderStatus
    {
        Draft,
        Submitted,
        Approved,
        Ordered,
        PartiallyReceived,
        Received,
        Cancelled
    }
}