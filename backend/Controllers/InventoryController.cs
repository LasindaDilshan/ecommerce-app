using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;
        private readonly ApplicationDbContext _context;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger, ApplicationDbContext context)
        {
            _inventoryService = inventoryService;
            _logger = logger;
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user identifier");
            }
            return userId;
        }

        // Warehouse Endpoints
        [HttpPost("warehouses")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request)
        {
            try
            {
                var warehouse = await _inventoryService.CreateWarehouseAsync(request);
                return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.WarehouseId }, warehouse);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                return StatusCode(500, new { message = "An error occurred while creating the warehouse" });
            }
        }

        [HttpGet("warehouses/{id}")]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            try
            {
                var warehouse = await _inventoryService.GetWarehouseByIdAsync(id);
                return Ok(warehouse);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("warehouses")]
        public async Task<IActionResult> GetAllWarehouses([FromQuery] bool activeOnly = true)
        {
            var warehouses = await _inventoryService.GetAllWarehousesAsync(activeOnly);
            return Ok(warehouses);
        }

        [HttpPut("warehouses/{id}")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] CreateWarehouseRequest request)
        {
            try
            {
                var warehouse = await _inventoryService.UpdateWarehouseAsync(id, request);
                return Ok(warehouse);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("warehouses/{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                var result = await _inventoryService.DeleteWarehouseAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("warehouses/{id}/set-default")]
        public async Task<IActionResult> SetDefaultWarehouse(int id)
        {
            var result = await _inventoryService.SetDefaultWarehouseAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new { message = "Default warehouse updated" });
        }

        // Stock Management Endpoints
        [HttpGet("stock/product/{productId}")]
        public async Task<IActionResult> GetStockByProduct(int productId)
        {
            var stock = await _inventoryService.GetStockByProductAsync(productId);
            return Ok(stock);
        }

        [HttpGet("stock/warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStockByWarehouse(int warehouseId)
        {
            var stock = await _inventoryService.GetStockByWarehouseAsync(warehouseId);
            return Ok(stock);
        }

        [HttpGet("stock/{productId}/{warehouseId}")]
        public async Task<IActionResult> GetStockItem(int productId, int warehouseId)
        {
            try
            {
                var stockItem = await _inventoryService.GetStockItemAsync(productId, warehouseId);
                return Ok(stockItem);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("stock/update")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest request)
        {
            try
            {
                var stockItem = await _inventoryService.UpdateStockAsync(request);
                return Ok(stockItem);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("stock/adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentRequest request)
        {
            try
            {
                var stockItem = await _inventoryService.AdjustStockAsync(request);
                return Ok(stockItem);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("stock/check-availability")]
        public async Task<IActionResult> CheckStockAvailability([FromQuery] int productId, [FromQuery] int quantity, [FromQuery] int? warehouseId = null)
        {
            var isAvailable = await _inventoryService.CheckStockAvailabilityAsync(productId, quantity, warehouseId);
            return Ok(new { available = isAvailable });
        }

        [HttpGet("stock/low-stock")]
        public async Task<IActionResult> GetLowStockItems([FromQuery] int? warehouseId = null)
        {
            var items = await _inventoryService.GetLowStockItemsAsync(warehouseId);
            return Ok(items);
        }

        // Stock Reservation Endpoints
        [HttpPost("stock/reserve")]
        public async Task<IActionResult> ReserveStock([FromBody] StockReservationRequest request)
        {
            var result = await _inventoryService.ReserveStockAsync(request);
            if (!result)
            {
                return BadRequest(new { message = "Insufficient stock available" });
            }
            return Ok(new { message = "Stock reserved successfully" });
        }

        [HttpDelete("stock/reservation/{reservationId}")]
        public async Task<IActionResult> ReleaseReservation(int reservationId)
        {
            var result = await _inventoryService.ReleaseReservationAsync(reservationId);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new { message = "Reservation released" });
        }

        [HttpPost("stock/reservation/{reservationId}/confirm")]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            var result = await _inventoryService.ConfirmReservationAsync(reservationId);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new { message = "Reservation confirmed" });
        }

        [HttpPost("stock/cleanup-reservations")]
        public async Task<IActionResult> CleanupExpiredReservations()
        {
            await _inventoryService.CleanupExpiredReservationsAsync();
            return Ok(new { message = "Expired reservations cleaned up" });
        }

        // Batch Management Endpoints
        [HttpPost("batches")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequest request)
        {
            try
            {
                var batch = await _inventoryService.CreateBatchAsync(request);
                return Ok(batch);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("batches/product/{productId}/warehouse/{warehouseId}")]
        public async Task<IActionResult> GetBatchesByProduct(int productId, int warehouseId)
        {
            var batches = await _inventoryService.GetBatchesByProductAsync(productId, warehouseId);
            return Ok(batches);
        }

        [HttpGet("batches/expiring")]
        public async Task<IActionResult> GetExpiringBatches([FromQuery] int daysAhead = 30)
        {
            var batches = await _inventoryService.GetExpiringBatchesAsync(daysAhead);
            return Ok(batches);
        }

        // Stock Transfer Endpoints
        [HttpPost("transfers")]
        public async Task<IActionResult> CreateStockTransfer([FromBody] CreateStockTransferRequest request)
        {
            try
            {
                var transfer = await _inventoryService.CreateStockTransferAsync(request);
                return CreatedAtAction(nameof(GetStockTransfer), new { id = transfer.TransferId }, transfer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("transfers/{id}")]
        public async Task<IActionResult> GetStockTransfer(int id)
        {
            try
            {
                var transfer = await _inventoryService.GetStockTransferAsync(id);
                return Ok(transfer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetStockTransfers([FromQuery] int? warehouseId = null)
        {
            var transfers = await _inventoryService.GetStockTransfersAsync(warehouseId);
            return Ok(transfers);
        }

        [HttpPost("transfers/{id}/approve")]
        public async Task<IActionResult> ApproveTransfer(int id)
        {
            try
            {
                var transfer = await _inventoryService.ApproveTransferAsync(id, GetUserId());
                return Ok(transfer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfers/{id}/ship")]
        public async Task<IActionResult> ShipTransfer(int id, [FromBody] string trackingNumber)
        {
            try
            {
                var transfer = await _inventoryService.ShipTransferAsync(id, trackingNumber);
                return Ok(transfer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfers/{id}/receive")]
        public async Task<IActionResult> ReceiveTransfer(int id, [FromBody] Dictionary<int, int> receivedQuantities)
        {
            try
            {
                var transfer = await _inventoryService.ReceiveTransferAsync(id, receivedQuantities);
                return Ok(transfer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("transfers/{id}")]
        public async Task<IActionResult> CancelTransfer(int id)
        {
            try
            {
                var result = await _inventoryService.CancelTransferAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Supplier Management Endpoints
        [HttpPost("suppliers")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
        {
            try
            {
                var supplier = await _inventoryService.CreateSupplierAsync(request);
                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierId }, supplier);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("suppliers/{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            try
            {
                var supplier = await _inventoryService.GetSupplierByIdAsync(id);
                return Ok(supplier);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] bool activeOnly = true)
        {
            var suppliers = await _inventoryService.GetAllSuppliersAsync(activeOnly);
            return Ok(suppliers);
        }

        [HttpPut("suppliers/{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] CreateSupplierRequest request)
        {
            try
            {
                var supplier = await _inventoryService.UpdateSupplierAsync(id, request);
                return Ok(supplier);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("suppliers/{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var result = await _inventoryService.DeleteSupplierAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("suppliers/{supplierId}/products/{productId}")]
        public async Task<IActionResult> LinkProductToSupplier(int supplierId, int productId, [FromBody] SupplierProductLinkRequest request)
        {
            var result = await _inventoryService.LinkProductToSupplierAsync(supplierId, productId, request.UnitCost, request.LeadTimeDays);
            if (!result)
            {
                return BadRequest();
            }
            return Ok(new { message = "Product linked to supplier" });
        }

        [HttpGet("suppliers/product/{productId}")]
        public async Task<IActionResult> GetSuppliersByProduct(int productId)
        {
            var suppliers = await _inventoryService.GetSuppliersByProductAsync(productId);
            return Ok(suppliers);
        }

        // Purchase Order Endpoints
        [HttpPost("purchase-orders")]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
        {
            var po = await _inventoryService.CreatePurchaseOrderAsync(request);
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.PurchaseOrderId }, po);
        }

        [HttpGet("purchase-orders/{id}")]
        public async Task<IActionResult> GetPurchaseOrder(int id)
        {
            try
            {
                var po = await _inventoryService.GetPurchaseOrderAsync(id);
                return Ok(po);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("purchase-orders")]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] PurchaseOrderStatus? status = null)
        {
            var orders = await _inventoryService.GetPurchaseOrdersAsync(status);
            return Ok(orders);
        }

        [HttpPost("purchase-orders/{id}/approve")]
        public async Task<IActionResult> ApprovePurchaseOrder(int id)
        {
            try
            {
                var po = await _inventoryService.ApprovePurchaseOrderAsync(id, GetUserId());
                return Ok(po);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("purchase-orders/{id}/receive")]
        public async Task<IActionResult> ReceivePurchaseOrder(int id, [FromBody] Dictionary<int, int> receivedQuantities)
        {
            try
            {
                var po = await _inventoryService.ReceivePurchaseOrderAsync(id, receivedQuantities);
                return Ok(po);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("purchase-orders/{id}")]
        public async Task<IActionResult> CancelPurchaseOrder(int id)
        {
            var result = await _inventoryService.CancelPurchaseOrderAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("purchase-orders/auto-generate")]
        public async Task<IActionResult> GenerateAutomaticPurchaseOrders()
        {
            var orders = await _inventoryService.GenerateAutomaticPurchaseOrdersAsync();
            return Ok(orders);
        }

        // Reports and Analytics
        [HttpGet("reports/overview")]
        public async Task<IActionResult> GetInventoryReport([FromQuery] int? warehouseId = null)
        {
            var report = await _inventoryService.GetInventoryReportAsync(warehouseId);
            return Ok(report);
        }

        [HttpGet("reports/stock-levels/{productId}")]
        public async Task<IActionResult> GetStockLevels(int productId)
        {
            var levels = await _inventoryService.GetStockLevelsAsync(productId);
            return Ok(levels);
        }

        [HttpGet("reports/inventory-value")]
        public async Task<IActionResult> GetInventoryValue([FromQuery] int? warehouseId = null)
        {
            var value = await _inventoryService.GetInventoryValueAsync(warehouseId);
            return Ok(new { totalValue = value });
        }

        [HttpGet("reports/reorder")]
        public async Task<IActionResult> GetReorderReport()
        {
            var items = await _inventoryService.GetReorderReportAsync();
            return Ok(items);
        }

        [HttpGet("reports/metrics")]
        public async Task<IActionResult> GetInventoryMetrics()
        {
            var metrics = await _inventoryService.GetInventoryMetricsAsync();
            return Ok(metrics);
        }

        [HttpGet("movements")]
        public async Task<IActionResult> GetStockMovements([FromQuery] int? productId = null, [FromQuery] int? warehouseId = null)
        {
            var movements = await _inventoryService.GetStockMovementsAsync(productId, warehouseId);
            return Ok(movements);
        }

        // Helper DTO for linking products to suppliers
        public class SupplierProductLinkRequest
        {
            public decimal UnitCost { get; set; }
            public int LeadTimeDays { get; set; }
        }

        // Initialize stock items from existing products
        [HttpPost("initialize-from-products")]
        public async Task<IActionResult> InitializeStockFromProducts()
        {
            try
            {
                _logger.LogInformation("Starting stock initialization from products");

                // Get or create default warehouse
                var warehouses = await _inventoryService.GetAllWarehousesAsync(true);
                var defaultWarehouse = warehouses.FirstOrDefault();

                if (defaultWarehouse == null)
                {
                    // Create a default warehouse
                    var warehouseRequest = new CreateWarehouseRequest
                    {
                        Name = "Main Warehouse",
                        Code = "MAIN",
                        Address = "123 Main Street",
                        City = "Default City",
                        State = "State",
                        Country = "Country",
                        ZipCode = "00000",
                        Phone = "",
                        Email = "",
                        IsDefault = true,
                        IsActive = true
                    };
                    defaultWarehouse = await _inventoryService.CreateWarehouseAsync(warehouseRequest);
                    _logger.LogInformation($"Created default warehouse: {defaultWarehouse.Name}");
                }

                // Get all products
                var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

                // Get existing stock items
                var existingStockItems = await _context.StockItems
                    .Where(s => s.WarehouseId == defaultWarehouse.WarehouseId)
                    .Select(s => s.ProductId)
                    .ToListAsync();

                int created = 0;
                int updated = 0;

                foreach (var product in products)
                {
                    if (!existingStockItems.Contains(product.Id))
                    {
                        // Create new stock item
                        var stockItem = new StockItem
                        {
                            ProductId = product.Id,
                            WarehouseId = defaultWarehouse.WarehouseId,
                            QuantityOnHand = product.StockQuantity,
                            QuantityReserved = 0,
                            ReorderPoint = 10,
                            ReorderQuantity = 50,
                            UnitCost = product.Price * 0.6m, // Estimate cost as 60% of price
                            LastRestockedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.StockItems.Add(stockItem);
                        created++;

                        // Record initial stock movement
                        var movement = new StockMovement
                        {
                            StockItemId = 0, // Will be set after SaveChanges
                            Type = StockMovementType.Adjustment,
                            Quantity = product.StockQuantity,
                            Reference = "Initial stock from product sync",
                            CreatedAt = DateTime.UtcNow
                        };
                        // We'll need to save first to get the stock item ID
                    }
                    else
                    {
                        // Update existing stock item
                        var stockItem = await _context.StockItems
                            .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == defaultWarehouse.WarehouseId);

                        if (stockItem != null && stockItem.QuantityOnHand != product.StockQuantity)
                        {
                            stockItem.QuantityOnHand = product.StockQuantity;
                            stockItem.UpdatedAt = DateTime.UtcNow;
                            updated++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Now create stock movements for the new items
                var newStockItems = await _context.StockItems
                    .Where(s => s.WarehouseId == defaultWarehouse.WarehouseId)
                    .ToListAsync();

                foreach (var stockItem in newStockItems)
                {
                    var hasMovement = await _context.StockMovements
                        .AnyAsync(m => m.StockItemId == stockItem.StockItemId);

                    if (!hasMovement && stockItem.QuantityOnHand > 0)
                    {
                        var movement = new StockMovement
                        {
                            StockItemId = stockItem.StockItemId,
                            Type = StockMovementType.Adjustment,
                            Quantity = stockItem.QuantityOnHand,
                            Reference = "Initial stock from product sync",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.StockMovements.Add(movement);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Stock initialization complete. Created: {created}, Updated: {updated}");

                return Ok(new
                {
                    message = "Stock initialization complete",
                    created,
                    updated,
                    warehouseId = defaultWarehouse.WarehouseId,
                    warehouseName = defaultWarehouse.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing stock from products");
                return StatusCode(500, new { message = "An error occurred while initializing stock" });
            }
        }
    }
}