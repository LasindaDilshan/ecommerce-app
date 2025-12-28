using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Exceptions;
using Backend.DTOs;

namespace EcommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscountCodeService _discountCodeService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        IDiscountCodeService discountCodeService,
        IInventoryService inventoryService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _discountCodeService = discountCodeService;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // Use a transaction to ensure atomicity and prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new EmptyCartException();
            }

            // Validate stock availability with optimistic concurrency
            // Uses EF Core tracking to detect concurrent modifications
            var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in cart.CartItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product) ||
                    product.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for product: {item.Product.Name}");
                }
                // Update the cart item's product reference with current data
                item.Product.StockQuantity = product.StockQuantity;
            }

        // Calculate totals with coupon support
        var subTotal = cart.CartItems.Sum(ci =>
            (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity);

        decimal discountAmount = 0;
        string? couponCode = null;
        DiscountCode? appliedCoupon = null;

        // Check if coupon code was provided or cart has coupon applied
        var requestCouponCode = request.CouponCode ?? cart.AppliedCouponCode;
        if (!string.IsNullOrEmpty(requestCouponCode))
        {
            appliedCoupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == requestCouponCode.ToUpper());

            if (appliedCoupon != null)
            {
                var cartProductIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
                var validation = await _discountCodeService.ValidateCouponAsync(
                    requestCouponCode,
                    userId,
                    null,
                    subTotal,
                    cartProductIds
                );

                if (validation.IsValid)
                {
                    discountAmount = validation.DiscountAmount;
                    couponCode = appliedCoupon.Code;
                }
            }
        }

        // Calculate final totals
        var discountedSubTotal = subTotal - discountAmount;
        var shippingCost = (appliedCoupon?.DiscountType == DiscountType.FreeShipping) ? 0 : 10.00m;
        var tax = discountedSubTotal * 0.08m; // 8% tax on discounted amount
        var totalAmount = discountedSubTotal + shippingCost + tax;

        // Create order
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            CouponCode = couponCode,
            ShippingCost = shippingCost,
            Tax = tax,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            OrderDate = DateTime.UtcNow,
            ShippingFirstName = request.ShippingAddress.FirstName,
            ShippingLastName = request.ShippingAddress.LastName,
            ShippingAddress = request.ShippingAddress.Address,
            ShippingCity = request.ShippingAddress.City,
            ShippingState = request.ShippingAddress.State,
            ShippingZipCode = request.ShippingAddress.ZipCode,
            ShippingCountry = request.ShippingAddress.Country,
            ShippingPhone = request.ShippingAddress.Phone
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items
        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.DiscountPrice ?? cartItem.Product.Price,
                TotalPrice = (cartItem.Product.DiscountPrice ?? cartItem.Product.Price) * cartItem.Quantity
            };

            _context.OrderItems.Add(orderItem);

            // Update product stock
            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        // Record discount usage if coupon was applied
        if (appliedCoupon != null && discountAmount > 0)
        {
            await _discountCodeService.IncrementUsageAsync(
                appliedCoupon.Id,
                userId,
                null,
                order.Id,
                discountAmount
            );
        }

        // Clear cart
        cart.CartItems.Clear();
        cart.AppliedCouponCode = null;
        cart.DiscountAmount = 0;
        cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Reload order with related data
            var createdOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstAsync(o => o.Id == order.Id);

            return MapToOrderDto(createdOrder);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return null;
        }

        return MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return null;
        }

        return MapToOrderDto(order);
    }

    public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                TotalItems = o.OrderItems.Sum(oi => oi.Quantity)
            })
            .ToListAsync();
    }

    public async Task<PagedResult<OrderDto>> GetAllOrdersAsync(int pageNumber, int pageSize)
    {
        // Validate and sanitize pagination parameters to prevent DoS
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100); // Max 100 items per page

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderDto>
        {
            Items = orders.Select(MapToOrderDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        var previousStatus = order.Status;

        // Handle cancellation specially
        if (request.Status == OrderStatus.Cancelled && previousStatus != OrderStatus.Cancelled)
        {
            await HandleOrderCancellationAsync(order);
        }

        order.Status = request.Status;

        if (request.Status == OrderStatus.Shipped)
        {
            order.ShippedDate = DateTime.UtcNow;
        }
        else if (request.Status == OrderStatus.Delivered)
        {
            order.DeliveredDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapToOrderDto(order);
    }

    private async Task HandleOrderCancellationAsync(Order order)
    {
        _logger.LogInformation($"Processing cancellation for order {order.OrderNumber}");

        // Validate cancellation is allowed
        if (order.Status == OrderStatus.Shipped)
        {
            throw new Exception("Cannot cancel an order that has already been shipped");
        }
        if (order.Status == OrderStatus.Delivered)
        {
            throw new Exception("Cannot cancel an order that has already been delivered");
        }

        // 1. Restore inventory for each order item
        foreach (var item in order.OrderItems)
        {
            try
            {
                // Restore stock to product
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _logger.LogInformation($"Restored {item.Quantity} units of product {product.Name} (ID: {product.Id})");

                    // Also record stock movement in inventory system for audit trail
                    try
                    {
                        // Get default warehouse
                        var warehouses = await _inventoryService.GetAllWarehousesAsync(true);
                        var defaultWarehouse = warehouses.FirstOrDefault();

                        if (defaultWarehouse != null)
                        {
                            // Get or create stock item
                            var stockItem = await _context.StockItems
                                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == defaultWarehouse.WarehouseId);

                            if (stockItem != null)
                            {
                                // Update stock item quantity
                                stockItem.QuantityOnHand += item.Quantity;

                                // Record stock movement for audit trail
                                await _inventoryService.RecordStockMovementAsync(
                                    stockItem.StockItemId,
                                    StockMovementType.Return,
                                    item.Quantity,
                                    $"Order #{order.OrderNumber} Cancelled"
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not record stock movement for product {product.Id}, but stock was restored");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring stock for product {item.ProductId}");
                throw;
            }
        }

        // 2. Process refund if payment was made
        if (order.PaymentStatus == PaymentStatus.Paid && !string.IsNullOrEmpty(order.PaymentIntentId))
        {
            try
            {
                // Get payment service to process refund
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
                if (payment != null)
                {
                    // Note: Full refund integration would require PaymentService injection
                    // For now, mark payment as refunded and log
                    order.PaymentStatus = PaymentStatus.Refunded;
                    _logger.LogInformation($"Marked order {order.OrderNumber} payment as refunded. Manual Stripe refund may be required.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for order {order.OrderNumber}");
                // Don't throw - cancellation should proceed even if refund fails
            }
        }

        // 3. Reverse discount code usage if coupon was applied
        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            try
            {
                var coupon = await _context.DiscountCodes
                    .FirstOrDefaultAsync(d => d.Code.ToUpper() == order.CouponCode.ToUpper());

                if (coupon != null)
                {
                    // Find and remove usage record
                    var usage = await _context.DiscountCodeUsages
                        .FirstOrDefaultAsync(u => u.DiscountCodeId == coupon.Id && u.OrderId == order.Id);

                    if (usage != null)
                    {
                        _context.DiscountCodeUsages.Remove(usage);
                        coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                        _logger.LogInformation($"Reversed discount code usage for coupon {coupon.Code} on order {order.OrderNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reversing discount for order {order.OrderNumber}");
                // Don't throw - cancellation should proceed
            }
        }

        _logger.LogInformation($"Successfully processed cancellation for order {order.OrderNumber}");
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerEmail = order.CustomerEmail,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            CouponCode = order.CouponCode,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                ProductImage = oi.Product.ImageUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            ShippingAddress = new ShippingAddressDto
            {
                FirstName = order.ShippingFirstName,
                LastName = order.ShippingLastName,
                Address = order.ShippingAddress,
                City = order.ShippingCity,
                State = order.ShippingState,
                ZipCode = order.ShippingZipCode,
                Country = order.ShippingCountry,
                Phone = order.ShippingPhone
            }
        };
    }

    // Guest order operations
    public async Task<GuestOrderResponse> CreateGuestOrderAsync(GuestCheckoutRequest request)
    {
        // Use a transaction to ensure atomicity and prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get guest cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.SessionId == request.SessionId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new EmptyCartException();
            }

            // Validate stock availability with optimistic concurrency
            var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in cart.CartItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product) ||
                    product.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for product: {item.Product.Name}");
                }
                item.Product.StockQuantity = product.StockQuantity;
            }

        // Calculate totals with coupon support
        var subTotal = cart.CartItems.Sum(ci =>
            (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity);

        decimal discountAmount = 0;
        string? couponCode = null;
        DiscountCode? appliedCoupon = null;

        // Check if coupon code was provided or cart has coupon applied
        var requestCouponCode = request.CouponCode ?? cart.AppliedCouponCode;
        if (!string.IsNullOrEmpty(requestCouponCode))
        {
            appliedCoupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == requestCouponCode.ToUpper());

            if (appliedCoupon != null)
            {
                var cartProductIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
                var validation = await _discountCodeService.ValidateCouponAsync(
                    requestCouponCode,
                    null,
                    request.Email,
                    subTotal,
                    cartProductIds
                );

                if (validation.IsValid)
                {
                    discountAmount = validation.DiscountAmount;
                    couponCode = appliedCoupon.Code;
                }
            }
        }

        // Calculate final totals
        var discountedSubTotal = subTotal - discountAmount;
        var shippingCost = (appliedCoupon?.DiscountType == DiscountType.FreeShipping) ? 0 : 10.00m;
        var tax = discountedSubTotal * 0.08m; // 8% tax on discounted amount
        var totalAmount = discountedSubTotal + shippingCost + tax;

        // Create guest order
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = null,
            GuestEmail = request.Email,
            GuestFirstName = request.FirstName,
            GuestLastName = request.LastName,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            CouponCode = couponCode,
            ShippingCost = shippingCost,
            Tax = tax,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            OrderDate = DateTime.UtcNow,
            ShippingFirstName = request.ShippingAddress.FirstName,
            ShippingLastName = request.ShippingAddress.LastName,
            ShippingAddress = request.ShippingAddress.Address,
            ShippingCity = request.ShippingAddress.City,
            ShippingState = request.ShippingAddress.State,
            ShippingZipCode = request.ShippingAddress.ZipCode,
            ShippingCountry = request.ShippingAddress.Country,
            ShippingPhone = request.ShippingAddress.Phone ?? request.PhoneNumber
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items
        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.DiscountPrice ?? cartItem.Product.Price,
                TotalPrice = (cartItem.Product.DiscountPrice ?? cartItem.Product.Price) * cartItem.Quantity
            };

            _context.OrderItems.Add(orderItem);

            // Update product stock
            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        // Record discount usage if coupon was applied
        if (appliedCoupon != null && discountAmount > 0)
        {
            await _discountCodeService.IncrementUsageAsync(
                appliedCoupon.Id,
                null,
                request.Email,
                order.Id,
                discountAmount
            );
        }

        // Clear guest cart
        _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Return guest order response
            return new GuestOrderResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Email = order.GuestEmail!,
                FirstName = order.GuestFirstName!,
                LastName = order.GuestLastName!,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                PaymentIntentId = order.PaymentIntentId,
                ClientSecret = null // Will be set by payment controller
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<GuestOrderResponse?> GetGuestOrderByNumberAsync(string orderNumber, string email)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.GuestEmail == email);

        if (order == null)
        {
            return null;
        }

        return new GuestOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Email = order.GuestEmail!,
            FirstName = order.GuestFirstName!,
            LastName = order.GuestLastName!,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            OrderDate = order.OrderDate,
            PaymentIntentId = order.PaymentIntentId,
            ClientSecret = null
        };
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}
