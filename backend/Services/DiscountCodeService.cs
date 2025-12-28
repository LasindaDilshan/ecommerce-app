using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public class DiscountCodeService : IDiscountCodeService
    {
        private readonly ApplicationDbContext _context;

        public DiscountCodeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates a coupon code and returns validation response with discount details
        /// </summary>
        public async Task<CouponValidationResponse> ValidateCouponAsync(
            string code,
            int? userId,
            string? guestEmail,
            decimal cartSubTotal,
            List<int> productIds)
        {
            var coupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper());

            if (coupon == null)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Coupon code not found"
                };
            }

            // Check if active
            if (!coupon.IsActive)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is no longer active"
                };
            }

            // Check expiry
            if (coupon.IsExpired)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has expired"
                };
            }

            // Check total usage limit
            if (coupon.IsUsageLimitReached)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has reached its usage limit"
                };
            }

            // Check per-user usage limit
            if (coupon.PerUserLimit.HasValue)
            {
                var userUsageCount = await GetUserUsageCountAsync(coupon.Id, userId, guestEmail);
                if (userUsageCount >= coupon.PerUserLimit.Value)
                {
                    return new CouponValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "You have already used this coupon the maximum number of times"
                    };
                }
            }

            // Check minimum purchase
            if (coupon.MinimumPurchase.HasValue && cartSubTotal < coupon.MinimumPurchase.Value)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Minimum purchase of ${coupon.MinimumPurchase.Value:F2} required"
                };
            }

            // Check product/category restrictions
            var eligibleProductIds = new List<int>();
            if (coupon.ApplicableProducts.Any() || coupon.ApplicableCategories.Any())
            {
                // Get products in cart that match restrictions
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.CategoryId })
                    .ToListAsync();

                foreach (var product in products)
                {
                    bool isEligible = false;

                    // Check if product is directly in applicable products
                    if (coupon.ApplicableProducts.Any(ap => ap.ProductId == product.Id))
                    {
                        isEligible = true;
                    }

                    // Check if product's category is in applicable categories
                    if (coupon.ApplicableCategories.Any(ac => ac.CategoryId == product.CategoryId))
                    {
                        isEligible = true;
                    }

                    if (isEligible)
                    {
                        eligibleProductIds.Add(product.Id);
                    }
                }

                if (!eligibleProductIds.Any())
                {
                    return new CouponValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "This coupon does not apply to any items in your cart"
                    };
                }
            }
            else
            {
                // No restrictions - all products are eligible
                eligibleProductIds = productIds;
            }

            // Calculate discount
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == (userId.HasValue
                    ? _context.Carts.Where(x => x.UserId == userId).Select(x => x.Id).FirstOrDefault()
                    : _context.Carts.Where(x => x.SessionId == guestEmail).Select(x => x.Id).FirstOrDefault()));

            decimal discountAmount = 0;
            if (cart != null)
            {
                // Filter cart items to only eligible ones
                var eligibleItems = cart.CartItems.Where(ci => eligibleProductIds.Contains(ci.ProductId)).ToList();
                discountAmount = await CalculateDiscountAsync(coupon, cartSubTotal, eligibleItems);
            }

            var finalTotal = Math.Max(0, cartSubTotal - discountAmount);

            return new CouponValidationResponse
            {
                IsValid = true,
                CouponCode = coupon.Code,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.Value,
                DiscountAmount = discountAmount,
                CartSubTotal = cartSubTotal,
                FinalTotal = finalTotal,
                EligibleProductIds = eligibleProductIds,
                SuccessMessage = $"Coupon applied! You saved ${discountAmount:F2}"
            };
        }

        /// <summary>
        /// Calculates discount amount based on coupon type
        /// </summary>
        public async Task<decimal> CalculateDiscountAsync(DiscountCode coupon, decimal cartSubTotal, List<CartItem> cartItems)
        {
            decimal discountAmount = 0;

            switch (coupon.DiscountType)
            {
                case DiscountType.Percentage:
                    // Calculate percentage discount
                    discountAmount = cartSubTotal * (coupon.Value / 100);

                    // Apply maximum discount cap if specified
                    if (coupon.MaximumDiscount.HasValue && discountAmount > coupon.MaximumDiscount.Value)
                    {
                        discountAmount = coupon.MaximumDiscount.Value;
                    }
                    break;

                case DiscountType.FixedAmount:
                    // Apply fixed amount discount (but not more than cart subtotal)
                    discountAmount = Math.Min(coupon.Value, cartSubTotal);
                    break;

                case DiscountType.FreeShipping:
                    // Shipping discount will be handled at checkout
                    // For cart display, we don't calculate shipping discount here
                    discountAmount = 0;
                    break;

                case DiscountType.BuyXGetY:
                    // Calculate Buy X Get Y discount
                    if (coupon.BuyQuantity.HasValue && coupon.GetQuantity.HasValue)
                    {
                        if (coupon.TargetProductId.HasValue)
                        {
                            // Specific product Buy X Get Y
                            var targetItem = cartItems.FirstOrDefault(ci => ci.ProductId == coupon.TargetProductId.Value);
                            if (targetItem != null)
                            {
                                int setsQualified = targetItem.Quantity / coupon.BuyQuantity.Value;
                                int freeItems = setsQualified * coupon.GetQuantity.Value;

                                var product = await _context.Products.FindAsync(targetItem.ProductId);
                                if (product != null)
                                {
                                    decimal itemPrice = product.DiscountPrice ?? product.Price;
                                    discountAmount = freeItems * itemPrice;
                                }
                            }
                        }
                        else
                        {
                            // Apply to cheapest items in cart
                            var totalQuantity = cartItems.Sum(ci => ci.Quantity);
                            int setsQualified = totalQuantity / coupon.BuyQuantity.Value;
                            int freeItems = setsQualified * coupon.GetQuantity.Value;

                            // Sort items by price (cheapest first) and apply discount
                            var sortedItems = cartItems
                                .Select(ci => new
                                {
                                    ci,
                                    Price = ci.Product.DiscountPrice ?? ci.Product.Price
                                })
                                .OrderBy(x => x.Price)
                                .ToList();

                            int itemsToDiscount = freeItems;
                            foreach (var item in sortedItems)
                            {
                                if (itemsToDiscount <= 0) break;

                                int itemsDiscounted = Math.Min(itemsToDiscount, item.ci.Quantity);
                                discountAmount += itemsDiscounted * item.Price;
                                itemsToDiscount -= itemsDiscounted;
                            }
                        }
                    }
                    break;
            }

            return Math.Round(discountAmount, 2);
        }

        /// <summary>
        /// Applies a coupon to a cart
        /// </summary>
        public async Task ApplyCouponToCartAsync(int cartId, string couponCode)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            var coupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == couponCode.ToUpper());

            if (coupon == null)
            {
                throw new Exception("Coupon code not found");
            }

            // Calculate subtotal
            var subTotal = cart.CartItems.Sum(ci =>
                (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity);

            // Get product IDs in cart
            var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

            // Validate coupon
            var validation = await ValidateCouponAsync(
                couponCode,
                cart.UserId,
                cart.SessionId,
                subTotal,
                productIds
            );

            if (!validation.IsValid)
            {
                throw new Exception(validation.ErrorMessage);
            }

            // Apply coupon to cart
            cart.AppliedCouponCode = coupon.Code;
            cart.DiscountAmount = validation.DiscountAmount;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes coupon from cart
        /// </summary>
        public async Task RemoveCouponFromCartAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Increments usage count and records usage
        /// </summary>
        public async Task IncrementUsageAsync(int discountCodeId, int? userId, string? guestEmail, int? orderId, decimal discountAmount)
        {
            var coupon = await _context.DiscountCodes.FindAsync(discountCodeId);
            if (coupon == null) return;

            // Increment total usage count
            coupon.UsedCount++;

            // Record individual usage
            var usage = new DiscountCodeUsage
            {
                DiscountCodeId = discountCodeId,
                UserId = userId,
                GuestEmail = guestEmail,
                OrderId = orderId,
                DiscountApplied = discountAmount,
                UsedAt = DateTime.UtcNow
            };

            _context.DiscountCodeUsages.Add(usage);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets usage count for a specific user/guest
        /// </summary>
        public async Task<int> GetUserUsageCountAsync(int discountCodeId, int? userId, string? guestEmail)
        {
            var query = _context.DiscountCodeUsages
                .Where(u => u.DiscountCodeId == discountCodeId);

            if (userId.HasValue)
            {
                query = query.Where(u => u.UserId == userId.Value);
            }
            else if (!string.IsNullOrEmpty(guestEmail))
            {
                query = query.Where(u => u.GuestEmail == guestEmail);
            }
            else
            {
                return 0;
            }

            return await query.CountAsync();
        }

        // Admin CRUD Operations

        public async Task<DiscountCodeDto> CreateDiscountCodeAsync(CreateDiscountCodeRequest request)
        {
            // Check if code already exists
            if (await _context.DiscountCodes.AnyAsync(d => d.Code.ToUpper() == request.Code.ToUpper()))
            {
                throw new Exception("A discount code with this code already exists");
            }

            var discountCode = new DiscountCode
            {
                Code = request.Code.ToUpper(),
                DiscountType = request.DiscountType,
                Value = request.Value,
                MinimumPurchase = request.MinimumPurchase,
                MaximumDiscount = request.MaximumDiscount,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                TotalUsageLimit = request.TotalUsageLimit,
                PerUserLimit = request.PerUserLimit,
                BuyQuantity = request.BuyQuantity,
                GetQuantity = request.GetQuantity,
                TargetProductId = request.TargetProductId,
                IsActive = request.IsActive,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.DiscountCodes.Add(discountCode);
            await _context.SaveChangesAsync();

            // Add product restrictions
            foreach (var productId in request.ApplicableProductIds)
            {
                _context.DiscountCodeProducts.Add(new DiscountCodeProduct
                {
                    DiscountCodeId = discountCode.Id,
                    ProductId = productId
                });
            }

            // Add category restrictions
            foreach (var categoryId in request.ApplicableCategoryIds)
            {
                _context.DiscountCodeCategories.Add(new DiscountCodeCategory
                {
                    DiscountCodeId = discountCode.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            return await GetDiscountCodeByIdAsync(discountCode.Id)
                ?? throw new Exception("Failed to retrieve created discount code");
        }

        public async Task<DiscountCodeDto> UpdateDiscountCodeAsync(int id, UpdateDiscountCodeRequest request)
        {
            var coupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (coupon == null)
            {
                throw new Exception("Discount code not found");
            }

            // Update fields
            if (request.Description != null) coupon.Description = request.Description;
            if (request.ValidFrom.HasValue) coupon.ValidFrom = request.ValidFrom.Value;
            if (request.ValidTo.HasValue) coupon.ValidTo = request.ValidTo.Value;
            if (request.IsActive.HasValue) coupon.IsActive = request.IsActive.Value;
            if (request.TotalUsageLimit.HasValue) coupon.TotalUsageLimit = request.TotalUsageLimit.Value;

            // Update product restrictions
            if (request.ApplicableProductIds != null)
            {
                coupon.ApplicableProducts.Clear();
                foreach (var productId in request.ApplicableProductIds)
                {
                    coupon.ApplicableProducts.Add(new DiscountCodeProduct
                    {
                        DiscountCodeId = coupon.Id,
                        ProductId = productId
                    });
                }
            }

            // Update category restrictions
            if (request.ApplicableCategoryIds != null)
            {
                coupon.ApplicableCategories.Clear();
                foreach (var categoryId in request.ApplicableCategoryIds)
                {
                    coupon.ApplicableCategories.Add(new DiscountCodeCategory
                    {
                        DiscountCodeId = coupon.Id,
                        CategoryId = categoryId
                    });
                }
            }

            coupon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetDiscountCodeByIdAsync(id)
                ?? throw new Exception("Failed to retrieve updated discount code");
        }

        public async Task<bool> DeleteDiscountCodeAsync(int id)
        {
            var coupon = await _context.DiscountCodes.FindAsync(id);
            if (coupon == null) return false;

            _context.DiscountCodes.Remove(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DiscountCodeDto?> GetDiscountCodeByIdAsync(int id)
        {
            var coupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (coupon == null) return null;

            return await MapToDtoAsync(coupon);
        }

        public async Task<DiscountCodeDto?> GetDiscountCodeByCodeAsync(string code)
        {
            var coupon = await _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper());

            if (coupon == null) return null;

            return await MapToDtoAsync(coupon);
        }

        public async Task<List<DiscountCodeDto>> GetAllDiscountCodesAsync(bool activeOnly = false)
        {
            var query = _context.DiscountCodes
                .Include(d => d.ApplicableProducts)
                .Include(d => d.ApplicableCategories)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(d => d.IsActive && !d.IsExpired && !d.IsUsageLimitReached);
            }

            var coupons = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
            var dtos = new List<DiscountCodeDto>();
            foreach (var coupon in coupons)
            {
                dtos.Add(await MapToDtoAsync(coupon));
            }
            return dtos;
        }

        public async Task<DiscountCodeStatsDto> GetStatsAsync()
        {
            var totalCodes = await _context.DiscountCodes.CountAsync();
            var activeCodes = await _context.DiscountCodes.CountAsync(d => d.IsActive);
            var expiredCodes = await _context.DiscountCodes.CountAsync(d => d.IsExpired);
            var totalUsages = await _context.DiscountCodeUsages.CountAsync();
            var totalDiscount = await _context.DiscountCodeUsages.SumAsync(u => (decimal?)u.DiscountApplied) ?? 0;

            return new DiscountCodeStatsDto
            {
                TotalCodes = totalCodes,
                ActiveCodes = activeCodes,
                ExpiredCodes = expiredCodes,
                TotalUsages = totalUsages,
                TotalDiscountGiven = totalDiscount
            };
        }

        // Helper method to map entity to DTO
        private async Task<DiscountCodeDto> MapToDtoAsync(DiscountCode coupon)
        {
            string? targetProductName = null;
            if (coupon.TargetProductId.HasValue)
            {
                targetProductName = await _context.Products
                    .Where(p => p.Id == coupon.TargetProductId.Value)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
            }

            return new DiscountCodeDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType,
                Value = coupon.Value,
                MinimumPurchase = coupon.MinimumPurchase,
                MaximumDiscount = coupon.MaximumDiscount,
                ValidFrom = coupon.ValidFrom,
                ValidTo = coupon.ValidTo,
                TotalUsageLimit = coupon.TotalUsageLimit,
                UsedCount = coupon.UsedCount,
                PerUserLimit = coupon.PerUserLimit,
                BuyQuantity = coupon.BuyQuantity,
                GetQuantity = coupon.GetQuantity,
                TargetProductId = coupon.TargetProductId,
                TargetProductName = targetProductName,
                IsActive = coupon.IsActive,
                Description = coupon.Description,
                CreatedAt = coupon.CreatedAt,
                ApplicableProductIds = coupon.ApplicableProducts.Select(ap => ap.ProductId).ToList(),
                ApplicableCategoryIds = coupon.ApplicableCategories.Select(ac => ac.CategoryId).ToList(),
                IsExpired = coupon.IsExpired,
                IsUsageLimitReached = coupon.IsUsageLimitReached,
                IsValid = coupon.IsValid
            };
        }
    }
}
