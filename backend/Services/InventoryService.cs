using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Warehouse Management
        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request)
        {
            // Check if code already exists
            if (await _context.Warehouses.AnyAsync(w => w.Code == request.Code))
            {
                throw new InvalidOperationException($"Warehouse with code {request.Code} already exists");
            }

            var warehouse = new Warehouse
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                Phone = request.Phone,
                Email = request.Email,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            // If this is set as default, remove default from others
            if (request.IsDefault)
            {
                var currentDefault = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Warehouse {warehouse.Name} created with ID {warehouse.WarehouseId}");

            return MapToWarehouseDto(warehouse);
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.StockItems)
                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);

            if (warehouse == null)
            {
                throw new ArgumentException($"Warehouse with ID {warehouseId} not found");
            }

            return MapToWarehouseDto(warehouse);
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync(bool activeOnly = true)
        {
            var query = _context.Warehouses
                .Include(w => w.StockItems)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(w => w.IsActive);
            }

            var warehouses = await query.ToListAsync();
            return warehouses.Select(MapToWarehouseDto);
        }

        public async Task<WarehouseDto> UpdateWarehouseAsync(int warehouseId, CreateWarehouseRequest request)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null)
            {
                throw new ArgumentException($"Warehouse with ID {warehouseId} not found");
            }

            warehouse.Name = request.Name;
            warehouse.Address = request.Address;
            warehouse.City = request.City;
            warehouse.State = request.State;
            warehouse.ZipCode = request.ZipCode;
            warehouse.Country = request.Country;
            warehouse.Phone = request.Phone;
            warehouse.Email = request.Email;
            warehouse.Latitude = request.Latitude;
            warehouse.Longitude = request.Longitude;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToWarehouseDto(warehouse);
        }

        public async Task<bool> DeleteWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.StockItems)
                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);

            if (warehouse == null)
            {
                return false;
            }

            if (warehouse.StockItems.Any())
            {
                throw new InvalidOperationException("Cannot delete warehouse with existing stock");
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse == null)
            {
                return false;
            }

            // Remove default from all warehouses
            await _context.Warehouses
                .Where(w => w.IsDefault)
                .ForEachAsync(w => w.IsDefault = false);

            warehouse.IsDefault = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // Stock Management
        public async Task<StockItemDto> GetStockItemAsync(int productId, int warehouseId)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (stockItem == null)
            {
                // Create new stock item with zero quantity
                var product = await _context.Products.FindAsync(productId);
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);

                if (product == null || warehouse == null)
                {
                    throw new ArgumentException("Product or warehouse not found");
                }

                stockItem = new StockItem
                {
                    ProductId = productId,
                    Product = product,
                    WarehouseId = warehouseId,
                    Warehouse = warehouse,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockItems.Add(stockItem);
                await _context.SaveChangesAsync();
            }

            return MapToStockItemDto(stockItem);
        }

        public async Task<IEnumerable<StockItemDto>> GetStockByProductAsync(int productId)
        {
            var stockItems = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Where(s => s.ProductId == productId)
                .ToListAsync();

            return stockItems.Select(MapToStockItemDto);
        }

        public async Task<IEnumerable<StockItemDto>> GetStockByWarehouseAsync(int warehouseId)
        {
            var stockItems = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Where(s => s.WarehouseId == warehouseId)
                .ToListAsync();

            return stockItems.Select(MapToStockItemDto);
        }

        public async Task<StockItemDto> UpdateStockAsync(UpdateStockRequest request)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.StockItemId == request.StockItemId);

            if (stockItem == null)
            {
                throw new ArgumentException("Stock item not found");
            }

            var balanceBefore = stockItem.QuantityOnHand;

            switch (request.Type)
            {
                case StockMovementType.Purchase:
                case StockMovementType.Return:
                case StockMovementType.Adjustment:
                    stockItem.QuantityOnHand += request.Quantity;
                    break;
                case StockMovementType.Sale:
                case StockMovementType.WriteOff:
                    if (stockItem.QuantityAvailable < request.Quantity)
                    {
                        throw new InvalidOperationException("Insufficient stock available");
                    }
                    stockItem.QuantityOnHand -= request.Quantity;
                    break;
                case StockMovementType.Recount:
                    stockItem.QuantityOnHand = request.Quantity;
                    break;
            }

            stockItem.UpdatedAt = DateTime.UtcNow;

            // Record movement
            var movement = new StockMovement
            {
                StockItemId = request.StockItemId,
                Type = request.Type,
                Quantity = request.Quantity,
                BalanceBefore = balanceBefore,
                BalanceAfter = stockItem.QuantityOnHand,
                Reference = request.Reference,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Stock updated for item {stockItem.StockItemId}: {balanceBefore} -> {stockItem.QuantityOnHand}");

            return MapToStockItemDto(stockItem);
        }

        public async Task<StockItemDto> AdjustStockAsync(StockAdjustmentRequest request)
        {
            var stockItem = await GetOrCreateStockItemAsync(request.ProductId, request.WarehouseId);

            var updateRequest = new UpdateStockRequest
            {
                StockItemId = stockItem.StockItemId,
                Quantity = request.NewQuantity,
                Type = StockMovementType.Recount,
                Notes = $"Stock adjustment: {request.Reason}. {request.Notes}"
            };

            return await UpdateStockAsync(updateRequest);
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity, int? warehouseId = null)
        {
            var query = _context.StockItems.Where(s => s.ProductId == productId);

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            var totalAvailable = await query.SumAsync(s => s.QuantityOnHand - s.QuantityReserved);

            return totalAvailable >= quantity;
        }

        public async Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync(int? warehouseId = null)
        {
            var query = _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Where(s => s.QuantityAvailable <= s.ReorderPoint);

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            var items = await query.ToListAsync();
            return items.Select(MapToStockItemDto);
        }

        // Stock Reservation
        public async Task<bool> ReserveStockAsync(StockReservationRequest request)
        {
            var stockItem = await GetOrCreateStockItemAsync(request.ProductId, request.WarehouseId);

            if (stockItem.QuantityAvailable < request.Quantity)
            {
                return false;
            }

            var reservation = new StockReservation
            {
                StockItemId = stockItem.StockItemId,
                OrderId = request.OrderId,
                CartItemId = request.CartItemId,
                Quantity = request.Quantity,
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes),
                Notes = request.Notes,
                IsActive = true
            };

            _context.StockReservations.Add(reservation);

            stockItem.QuantityReserved += request.Quantity;
            stockItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reserved {request.Quantity} units of product {request.ProductId} in warehouse {request.WarehouseId}");

            return true;
        }

        public async Task<bool> ReleaseReservationAsync(int reservationId)
        {
            var reservation = await _context.StockReservations
                .Include(r => r.StockItem)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null || !reservation.IsActive)
            {
                return false;
            }

            reservation.IsActive = false;
            reservation.StockItem.QuantityReserved -= reservation.Quantity;
            reservation.StockItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _context.StockReservations
                .Include(r => r.StockItem)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null || !reservation.IsActive)
            {
                return false;
            }

            // Convert reservation to actual stock reduction
            reservation.StockItem.QuantityOnHand -= reservation.Quantity;
            reservation.StockItem.QuantityReserved -= reservation.Quantity;
            reservation.IsActive = false;

            // Record movement
            var movement = new StockMovement
            {
                StockItemId = reservation.StockItemId,
                Type = StockMovementType.Sale,
                Quantity = reservation.Quantity,
                BalanceBefore = reservation.StockItem.QuantityOnHand + reservation.Quantity,
                BalanceAfter = reservation.StockItem.QuantityOnHand,
                OrderId = reservation.OrderId,
                Reference = $"Order #{reservation.OrderId}",
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task CleanupExpiredReservationsAsync()
        {
            var expiredReservations = await _context.StockReservations
                .Include(r => r.StockItem)
                .Where(r => r.IsActive && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                reservation.IsActive = false;
                reservation.StockItem.QuantityReserved -= reservation.Quantity;
            }

            if (expiredReservations.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {expiredReservations.Count} expired reservations");
            }
        }

        // Batch Management
        public async Task<StockBatchDto> CreateBatchAsync(CreateBatchRequest request)
        {
            var stockItem = await _context.StockItems.FindAsync(request.StockItemId);
            if (stockItem == null)
            {
                throw new ArgumentException("Stock item not found");
            }

            var batch = new StockBatch
            {
                StockItemId = request.StockItemId,
                BatchNumber = request.BatchNumber,
                LotNumber = request.LotNumber,
                Quantity = request.Quantity,
                QuantityRemaining = request.Quantity,
                ManufactureDate = request.ManufactureDate,
                ExpiryDate = request.ExpiryDate,
                SupplierId = request.SupplierId,
                PurchaseCost = request.PurchaseCost,
                ReceivedDate = DateTime.UtcNow,
                Notes = request.Notes,
                IsActive = true
            };

            _context.StockBatches.Add(batch);

            // Update stock quantity
            stockItem.QuantityOnHand += request.Quantity;
            stockItem.LastRestockedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToStockBatchDto(batch);
        }

        public async Task<IEnumerable<StockBatchDto>> GetBatchesByProductAsync(int productId, int warehouseId)
        {
            var batches = await _context.StockBatches
                .Include(b => b.StockItem)
                .Include(b => b.Supplier)
                .Where(b => b.StockItem.ProductId == productId &&
                           b.StockItem.WarehouseId == warehouseId &&
                           b.IsActive)
                .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue)
                .ToListAsync();

            return batches.Select(MapToStockBatchDto);
        }

        public async Task<IEnumerable<StockBatchDto>> GetExpiringBatchesAsync(int daysAhead = 30)
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysAhead);

            var batches = await _context.StockBatches
                .Include(b => b.StockItem)
                .ThenInclude(s => s.Product)
                .Include(b => b.Supplier)
                .Where(b => b.IsActive &&
                           b.ExpiryDate.HasValue &&
                           b.ExpiryDate.Value <= expiryDate &&
                           b.QuantityRemaining > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            return batches.Select(MapToStockBatchDto);
        }

        public async Task<bool> UpdateBatchAsync(int batchId, int newQuantity)
        {
            var batch = await _context.StockBatches
                .Include(b => b.StockItem)
                .FirstOrDefaultAsync(b => b.BatchId == batchId);

            if (batch == null)
            {
                return false;
            }

            var difference = newQuantity - batch.QuantityRemaining;
            batch.QuantityRemaining = newQuantity;
            batch.StockItem.QuantityOnHand += difference;

            await _context.SaveChangesAsync();

            return true;
        }

        // Stock Transfers
        public async Task<StockTransferDto> CreateStockTransferAsync(CreateStockTransferRequest request)
        {
            if (request.FromWarehouseId == request.ToWarehouseId)
            {
                throw new InvalidOperationException("Cannot transfer to the same warehouse");
            }

            var transfer = new StockTransfer
            {
                TransferNumber = GenerateTransferNumber(),
                FromWarehouseId = request.FromWarehouseId,
                ToWarehouseId = request.ToWarehouseId,
                Status = StockTransferStatus.Pending,
                RequestedDate = DateTime.UtcNow,
                Notes = request.Notes
            };

            foreach (var item in request.Items)
            {
                var stockItem = await GetOrCreateStockItemAsync(item.ProductId, request.FromWarehouseId);

                if (stockItem.QuantityAvailable < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
                }

                var transferItem = new StockTransferItem
                {
                    ProductId = item.ProductId,
                    RequestedQuantity = item.Quantity,
                    BatchNumber = item.BatchNumber
                };

                transfer.TransferItems.Add(transferItem);

                // Reserve stock
                stockItem.QuantityReserved += item.Quantity;
            }

            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            return await GetStockTransferAsync(transfer.TransferId);
        }

        public async Task<StockTransferDto> GetStockTransferAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .Include(t => t.TransferItems)
                .ThenInclude(ti => ti.Product)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                throw new ArgumentException("Transfer not found");
            }

            return MapToStockTransferDto(transfer);
        }

        public async Task<IEnumerable<StockTransferDto>> GetStockTransfersAsync(int? warehouseId = null)
        {
            var query = _context.StockTransfers
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .Include(t => t.TransferItems)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(t => t.FromWarehouseId == warehouseId.Value ||
                                         t.ToWarehouseId == warehouseId.Value);
            }

            var transfers = await query.OrderByDescending(t => t.RequestedDate).ToListAsync();

            return transfers.Select(MapToStockTransferDto);
        }

        public async Task<StockTransferDto> ApproveTransferAsync(int transferId, int approvedByUserId)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                throw new ArgumentException("Transfer not found");
            }

            if (transfer.Status != StockTransferStatus.Pending)
            {
                throw new InvalidOperationException("Transfer is not pending approval");
            }

            transfer.Status = StockTransferStatus.Approved;
            transfer.ApprovedDate = DateTime.UtcNow;
            transfer.ApprovedByUserId = approvedByUserId;

            await _context.SaveChangesAsync();

            return await GetStockTransferAsync(transferId);
        }

        public async Task<StockTransferDto> ShipTransferAsync(int transferId, string trackingNumber)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                throw new ArgumentException("Transfer not found");
            }

            if (transfer.Status != StockTransferStatus.Approved)
            {
                throw new InvalidOperationException("Transfer must be approved before shipping");
            }

            transfer.Status = StockTransferStatus.InTransit;
            transfer.ShippedDate = DateTime.UtcNow;
            transfer.TrackingNumber = trackingNumber;

            // Update shipped quantities
            foreach (var item in transfer.TransferItems)
            {
                item.ShippedQuantity = item.RequestedQuantity;

                // Reduce stock from source warehouse
                var stockItem = await GetOrCreateStockItemAsync(item.ProductId, transfer.FromWarehouseId);
                stockItem.QuantityOnHand -= item.RequestedQuantity;
                stockItem.QuantityReserved -= item.RequestedQuantity;
            }

            await _context.SaveChangesAsync();

            return await GetStockTransferAsync(transferId);
        }

        public async Task<StockTransferDto> ReceiveTransferAsync(int transferId, Dictionary<int, int> receivedQuantities)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                throw new ArgumentException("Transfer not found");
            }

            if (transfer.Status != StockTransferStatus.InTransit)
            {
                throw new InvalidOperationException("Transfer is not in transit");
            }

            transfer.Status = StockTransferStatus.Received;
            transfer.ReceivedDate = DateTime.UtcNow;

            foreach (var item in transfer.TransferItems)
            {
                if (receivedQuantities.ContainsKey(item.ProductId))
                {
                    item.ReceivedQuantity = receivedQuantities[item.ProductId];

                    // Add stock to destination warehouse
                    var destStockItem = await GetOrCreateStockItemAsync(item.ProductId, transfer.ToWarehouseId);
                    destStockItem.QuantityOnHand += item.ReceivedQuantity;
                    destStockItem.LastRestockedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return await GetStockTransferAsync(transferId);
        }

        public async Task<bool> CancelTransferAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.TransferItems)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return false;
            }

            if (transfer.Status == StockTransferStatus.Received)
            {
                throw new InvalidOperationException("Cannot cancel received transfer");
            }

            // Release reserved stock if not yet shipped
            if (transfer.Status == StockTransferStatus.Pending || transfer.Status == StockTransferStatus.Approved)
            {
                foreach (var item in transfer.TransferItems)
                {
                    var stockItem = await GetOrCreateStockItemAsync(item.ProductId, transfer.FromWarehouseId);
                    stockItem.QuantityReserved -= item.RequestedQuantity;
                }
            }

            transfer.Status = StockTransferStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }

        // Supplier Management
        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request)
        {
            if (await _context.Suppliers.AnyAsync(s => s.Code == request.Code))
            {
                throw new InvalidOperationException($"Supplier with code {request.Code} already exists");
            }

            var supplier = new Supplier
            {
                Name = request.Name,
                Code = request.Code,
                ContactPerson = request.ContactPerson,
                Email = request.Email,
                Phone = request.Phone,
                Website = request.Website,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                TaxId = request.TaxId,
                PaymentTermsDays = request.PaymentTermsDays,
                DiscountPercentage = request.DiscountPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return MapToSupplierDto(supplier);
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(int supplierId)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierProducts)
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

            if (supplier == null)
            {
                throw new ArgumentException("Supplier not found");
            }

            return MapToSupplierDto(supplier);
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(bool activeOnly = true)
        {
            var query = _context.Suppliers
                .Include(s => s.SupplierProducts)
                .Include(s => s.PurchaseOrders)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            var suppliers = await query.ToListAsync();
            return suppliers.Select(MapToSupplierDto);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int supplierId, CreateSupplierRequest request)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                throw new ArgumentException("Supplier not found");
            }

            supplier.Name = request.Name;
            supplier.ContactPerson = request.ContactPerson;
            supplier.Email = request.Email;
            supplier.Phone = request.Phone;
            supplier.Website = request.Website;
            supplier.Address = request.Address;
            supplier.City = request.City;
            supplier.State = request.State;
            supplier.ZipCode = request.ZipCode;
            supplier.Country = request.Country;
            supplier.TaxId = request.TaxId;
            supplier.PaymentTermsDays = request.PaymentTermsDays;
            supplier.DiscountPercentage = request.DiscountPercentage;
            supplier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToSupplierDto(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                return false;
            }

            supplier.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LinkProductToSupplierAsync(int supplierId, int productId, decimal unitCost, int leadTimeDays)
        {
            var supplierProduct = await _context.SupplierProducts
                .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId && sp.ProductId == productId);

            if (supplierProduct == null)
            {
                supplierProduct = new SupplierProduct
                {
                    SupplierId = supplierId,
                    ProductId = productId,
                    UnitCost = unitCost,
                    LeadTimeDays = leadTimeDays,
                    CreatedAt = DateTime.UtcNow
                };
                _context.SupplierProducts.Add(supplierProduct);
            }
            else
            {
                supplierProduct.UnitCost = unitCost;
                supplierProduct.LeadTimeDays = leadTimeDays;
                supplierProduct.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SupplierDto>> GetSuppliersByProductAsync(int productId)
        {
            var suppliers = await _context.SupplierProducts
                .Include(sp => sp.Supplier)
                .Where(sp => sp.ProductId == productId && sp.IsActive)
                .Select(sp => sp.Supplier)
                .Distinct()
                .ToListAsync();

            return suppliers.Select(MapToSupplierDto);
        }

        // Purchase Orders - Basic implementation
        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request)
        {
            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = GeneratePurchaseOrderNumber(),
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                Status = PurchaseOrderStatus.Draft,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Notes = request.Notes
            };

            decimal subTotal = 0;
            foreach (var item in request.Items)
            {
                var poItem = new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    OrderedQuantity = item.Quantity,
                    UnitCost = item.UnitCost
                };
                purchaseOrder.PurchaseOrderItems.Add(poItem);
                subTotal += item.Quantity * item.UnitCost;
            }

            purchaseOrder.SubTotal = subTotal;
            purchaseOrder.TotalAmount = subTotal; // Can add tax and shipping later

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            return await GetPurchaseOrderAsync(purchaseOrder.PurchaseOrderId);
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderAsync(int purchaseOrderId)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .Include(p => p.PurchaseOrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

            if (po == null)
            {
                throw new ArgumentException("Purchase order not found");
            }

            return MapToPurchaseOrderDto(po);
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(PurchaseOrderStatus? status = null)
        {
            var query = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .Include(p => p.PurchaseOrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            var orders = await query.OrderByDescending(p => p.OrderDate).ToListAsync();

            return orders.Select(MapToPurchaseOrderDto);
        }

        public async Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(int purchaseOrderId, int approvedByUserId)
        {
            var po = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (po == null)
            {
                throw new ArgumentException("Purchase order not found");
            }

            po.Status = PurchaseOrderStatus.Approved;
            po.ApprovedByUserId = approvedByUserId;
            await _context.SaveChangesAsync();

            return await GetPurchaseOrderAsync(purchaseOrderId);
        }

        public async Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> receivedQuantities)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.PurchaseOrderItems)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

            if (po == null)
            {
                throw new ArgumentException("Purchase order not found");
            }

            foreach (var item in po.PurchaseOrderItems)
            {
                if (receivedQuantities.ContainsKey(item.ProductId))
                {
                    item.ReceivedQuantity = receivedQuantities[item.ProductId];

                    // Update stock
                    var stockItem = await GetOrCreateStockItemAsync(item.ProductId, po.WarehouseId);
                    stockItem.QuantityOnHand += item.ReceivedQuantity;
                    stockItem.UnitCost = item.UnitCost;
                    stockItem.LastRestockedAt = DateTime.UtcNow;
                }
            }

            po.Status = PurchaseOrderStatus.Received;
            po.ReceivedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPurchaseOrderAsync(purchaseOrderId);
        }

        public async Task<bool> CancelPurchaseOrderAsync(int purchaseOrderId)
        {
            var po = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (po == null)
            {
                return false;
            }

            po.Status = PurchaseOrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GenerateAutomaticPurchaseOrdersAsync()
        {
            var lowStockItems = await GetLowStockItemsAsync();
            var purchaseOrders = new List<PurchaseOrderDto>();

            // Group by preferred supplier and create POs
            // This is a simplified implementation
            foreach (var item in lowStockItems)
            {
                var supplierProduct = await _context.SupplierProducts
                    .Include(sp => sp.Supplier)
                    .Where(sp => sp.ProductId == item.ProductId && sp.IsActive && sp.IsPreferred)
                    .FirstOrDefaultAsync();

                if (supplierProduct != null)
                {
                    var request = new CreatePurchaseOrderRequest
                    {
                        SupplierId = supplierProduct.SupplierId,
                        WarehouseId = item.WarehouseId,
                        Items = new List<PurchaseOrderItemRequest>
                        {
                            new PurchaseOrderItemRequest
                            {
                                ProductId = item.ProductId,
                                Quantity = item.ReorderQuantity,
                                UnitCost = supplierProduct.UnitCost
                            }
                        }
                    };

                    var po = await CreatePurchaseOrderAsync(request);
                    purchaseOrders.Add(po);
                }
            }

            return purchaseOrders;
        }

        // Stock Movement History
        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int? productId = null, int? warehouseId = null)
        {
            var query = _context.StockMovements
                .Include(m => m.StockItem)
                .ThenInclude(s => s.Product)
                .Include(m => m.StockItem.Warehouse)
                .Include(m => m.User)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(m => m.StockItem.ProductId == productId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(m => m.StockItem.WarehouseId == warehouseId.Value);
            }

            var movements = await query.OrderByDescending(m => m.CreatedAt).Take(100).ToListAsync();

            return movements.Select(MapToStockMovementDto);
        }

        public async Task RecordStockMovementAsync(int stockItemId, StockMovementType type, int quantity, string? reference = null)
        {
            var stockItem = await _context.StockItems.FindAsync(stockItemId);
            if (stockItem == null)
            {
                return;
            }

            var movement = new StockMovement
            {
                StockItemId = stockItemId,
                Type = type,
                Quantity = quantity,
                BalanceBefore = stockItem.QuantityOnHand,
                BalanceAfter = stockItem.QuantityOnHand,
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
        }

        // Reports
        public async Task<InventoryReportDto> GetInventoryReportAsync(int? warehouseId = null)
        {
            var query = _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Include(s => s.Batches)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            var stockItems = await query.ToListAsync();

            var report = new InventoryReportDto
            {
                TotalProducts = stockItems.Select(s => s.ProductId).Distinct().Count(),
                TotalStock = stockItems.Sum(s => s.QuantityOnHand),
                TotalValue = stockItems.Sum(s => s.QuantityOnHand * s.UnitCost),
                LowStockItems = stockItems.Count(s => s.QuantityAvailable <= s.ReorderPoint),
                OutOfStockItems = stockItems.Count(s => s.QuantityAvailable == 0),
                LowStockProducts = stockItems
                    .Where(s => s.QuantityAvailable <= s.ReorderPoint)
                    .Select(MapToStockItemDto)
                    .ToList()
            };

            // Get expiring batches
            var expiringBatches = await GetExpiringBatchesAsync(30);
            report.ExpiringItems = expiringBatches.Count();
            report.ExpiringBatches = expiringBatches.ToList();

            return report;
        }

        public async Task<Dictionary<int, int>> GetStockLevelsAsync(int productId)
        {
            var stockLevels = await _context.StockItems
                .Where(s => s.ProductId == productId)
                .ToDictionaryAsync(s => s.WarehouseId, s => s.QuantityAvailable);

            return stockLevels;
        }

        public async Task<decimal> GetInventoryValueAsync(int? warehouseId = null)
        {
            var query = _context.StockItems.AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            return await query.SumAsync(s => s.QuantityOnHand * s.UnitCost);
        }

        public async Task<IEnumerable<StockItemDto>> GetReorderReportAsync()
        {
            var items = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Where(s => s.QuantityAvailable <= s.ReorderPoint)
                .ToListAsync();

            return items.Select(MapToStockItemDto);
        }

        public async Task<Dictionary<string, object>> GetInventoryMetricsAsync()
        {
            var metrics = new Dictionary<string, object>();

            metrics["totalWarehouses"] = await _context.Warehouses.CountAsync(w => w.IsActive);
            metrics["totalProducts"] = await _context.Products.CountAsync();
            metrics["totalStock"] = await _context.StockItems.SumAsync(s => s.QuantityOnHand);
            metrics["totalValue"] = await _context.StockItems.SumAsync(s => s.QuantityOnHand * s.UnitCost);
            metrics["lowStockAlerts"] = await _context.StockItems.CountAsync(s => s.QuantityAvailable <= s.ReorderPoint);
            metrics["pendingTransfers"] = await _context.StockTransfers.CountAsync(t => t.Status == StockTransferStatus.Pending);
            metrics["pendingPurchaseOrders"] = await _context.PurchaseOrders.CountAsync(p => p.Status == PurchaseOrderStatus.Draft);

            return metrics;
        }

        // Helper Methods
        private async Task<StockItem> GetOrCreateStockItemAsync(int productId, int warehouseId)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (stockItem == null)
            {
                stockItem = new StockItem
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockItems.Add(stockItem);
                await _context.SaveChangesAsync();

                stockItem = await _context.StockItems
                    .Include(s => s.Product)
                    .Include(s => s.Warehouse)
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
            }

            return stockItem!;
        }

        private string GenerateTransferNumber()
        {
            return $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        private string GeneratePurchaseOrderNumber()
        {
            return $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        // Mapping Methods
        private WarehouseDto MapToWarehouseDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                WarehouseId = warehouse.WarehouseId,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = warehouse.Address,
                City = warehouse.City,
                State = warehouse.State,
                ZipCode = warehouse.ZipCode,
                Country = warehouse.Country,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                IsActive = warehouse.IsActive,
                IsDefault = warehouse.IsDefault,
                Latitude = warehouse.Latitude,
                Longitude = warehouse.Longitude,
                TotalProducts = warehouse.StockItems?.Select(s => s.ProductId).Distinct().Count() ?? 0,
                TotalStock = warehouse.StockItems?.Sum(s => s.QuantityOnHand) ?? 0,
                TotalValue = warehouse.StockItems?.Sum(s => s.QuantityOnHand * s.UnitCost) ?? 0
            };
        }

        private StockItemDto MapToStockItemDto(StockItem stockItem)
        {
            return new StockItemDto
            {
                StockItemId = stockItem.StockItemId,
                ProductId = stockItem.ProductId,
                ProductName = stockItem.Product?.Name ?? "",
                ProductSKU = stockItem.Product?.SKU ?? "",
                WarehouseId = stockItem.WarehouseId,
                WarehouseName = stockItem.Warehouse?.Name ?? "",
                QuantityOnHand = stockItem.QuantityOnHand,
                QuantityReserved = stockItem.QuantityReserved,
                QuantityAvailable = stockItem.QuantityAvailable,
                ReorderPoint = stockItem.ReorderPoint,
                ReorderQuantity = stockItem.ReorderQuantity,
                UnitCost = stockItem.UnitCost,
                TotalValue = stockItem.QuantityOnHand * stockItem.UnitCost,
                NeedsReorder = stockItem.QuantityAvailable <= stockItem.ReorderPoint,
                LastRestockedAt = stockItem.LastRestockedAt,
                LastSoldAt = stockItem.LastSoldAt
            };
        }

        private StockBatchDto MapToStockBatchDto(StockBatch batch)
        {
            return new StockBatchDto
            {
                BatchId = batch.BatchId,
                BatchNumber = batch.BatchNumber,
                LotNumber = batch.LotNumber,
                Quantity = batch.Quantity,
                QuantityRemaining = batch.QuantityRemaining,
                ManufactureDate = batch.ManufactureDate,
                ExpiryDate = batch.ExpiryDate,
                SupplierName = batch.Supplier?.Name,
                PurchaseCost = batch.PurchaseCost,
                ReceivedDate = batch.ReceivedDate,
                IsExpired = batch.ExpiryDate.HasValue && batch.ExpiryDate.Value < DateTime.UtcNow,
                IsExpiringSoon = batch.ExpiryDate.HasValue && batch.ExpiryDate.Value < DateTime.UtcNow.AddDays(30)
            };
        }

        private StockTransferDto MapToStockTransferDto(StockTransfer transfer)
        {
            return new StockTransferDto
            {
                TransferId = transfer.TransferId,
                TransferNumber = transfer.TransferNumber,
                FromWarehouseId = transfer.FromWarehouseId,
                FromWarehouseName = transfer.FromWarehouse?.Name ?? "",
                ToWarehouseId = transfer.ToWarehouseId,
                ToWarehouseName = transfer.ToWarehouse?.Name ?? "",
                Status = transfer.Status,
                RequestedDate = transfer.RequestedDate,
                ShippedDate = transfer.ShippedDate,
                ReceivedDate = transfer.ReceivedDate,
                TrackingNumber = transfer.TrackingNumber,
                Items = transfer.TransferItems?.Select(i => new StockTransferItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    ProductSKU = i.Product?.SKU ?? "",
                    RequestedQuantity = i.RequestedQuantity,
                    ShippedQuantity = i.ShippedQuantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    BatchNumber = i.BatchNumber
                }).ToList() ?? new List<StockTransferItemDto>(),
                TotalItems = transfer.TransferItems?.Count ?? 0,
                TotalQuantity = transfer.TransferItems?.Sum(i => i.RequestedQuantity) ?? 0
            };
        }

        private SupplierDto MapToSupplierDto(Supplier supplier)
        {
            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Code = supplier.Code,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Website = supplier.Website,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                Country = supplier.Country,
                PaymentTermsDays = supplier.PaymentTermsDays,
                DiscountPercentage = supplier.DiscountPercentage,
                IsActive = supplier.IsActive,
                TotalProducts = supplier.SupplierProducts?.Count ?? 0,
                TotalOrders = supplier.PurchaseOrders?.Count ?? 0
            };
        }

        private PurchaseOrderDto MapToPurchaseOrderDto(PurchaseOrder po)
        {
            return new PurchaseOrderDto
            {
                PurchaseOrderId = po.PurchaseOrderId,
                OrderNumber = po.OrderNumber,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier?.Name ?? "",
                WarehouseId = po.WarehouseId,
                WarehouseName = po.Warehouse?.Name ?? "",
                Status = po.Status,
                SubTotal = po.SubTotal,
                TaxAmount = po.TaxAmount,
                ShippingCost = po.ShippingCost,
                TotalAmount = po.TotalAmount,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                ReceivedDate = po.ReceivedDate,
                Items = po.PurchaseOrderItems?.Select(i => new PurchaseOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    ProductSKU = i.Product?.SKU ?? "",
                    OrderedQuantity = i.OrderedQuantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    UnitCost = i.UnitCost,
                    TotalCost = i.OrderedQuantity * i.UnitCost
                }).ToList() ?? new List<PurchaseOrderItemDto>()
            };
        }

        private StockMovementDto MapToStockMovementDto(StockMovement movement)
        {
            return new StockMovementDto
            {
                MovementId = movement.MovementId,
                StockItemId = movement.StockItemId,
                ProductName = movement.StockItem?.Product?.Name ?? "",
                WarehouseName = movement.StockItem?.Warehouse?.Name ?? "",
                Type = movement.Type,
                Quantity = movement.Quantity,
                BalanceBefore = movement.BalanceBefore,
                BalanceAfter = movement.BalanceAfter,
                Reference = movement.Reference,
                UserName = movement.User != null ? $"{movement.User.FirstName} {movement.User.LastName}" : null,
                Notes = movement.Notes,
                CreatedAt = movement.CreatedAt
            };
        }
    }
}