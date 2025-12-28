using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Backend.Services;

namespace EcommerceAPI.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IEmailService _emailService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public SubscriptionService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<SubscriptionService> logger,
            IEmailService emailService,
            IPaymentService paymentService,
            IOrderService orderService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _paymentService = paymentService;
            _orderService = orderService;
        }

        #region Subscription Plan Management

        public async Task<IEnumerable<SubscriptionPlanDto>> GetAllPlansAsync(bool activeOnly = true)
        {
            var query = _context.SubscriptionPlans
                .Include(p => p.PlanProducts)
                    .ThenInclude(pp => pp.Product)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(p => p.IsActive && p.IsVisible);
            }

            var plans = await query
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            var planDtos = new List<SubscriptionPlanDto>();
            foreach (var plan in plans)
            {
                var dto = _mapper.Map<SubscriptionPlanDto>(plan);
                if (!string.IsNullOrEmpty(plan.FeaturesJson))
                {
                    dto.Features = JsonConvert.DeserializeObject<List<string>>(plan.FeaturesJson) ?? new List<string>();
                }
                dto.Products = plan.PlanProducts.Select(pp => new SubscriptionPlanProductDto
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product.Name,
                    ProductSKU = pp.Product.SKU,
                    Quantity = pp.Quantity,
                    DiscountPercentage = pp.DiscountPercentage,
                    IsOptional = pp.IsOptional
                }).ToList();
                planDtos.Add(dto);
            }

            return planDtos;
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.PlanProducts)
                    .ThenInclude(pp => pp.Product)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null) return null;

            var dto = _mapper.Map<SubscriptionPlanDto>(plan);
            if (!string.IsNullOrEmpty(plan.FeaturesJson))
            {
                dto.Features = JsonConvert.DeserializeObject<List<string>>(plan.FeaturesJson) ?? new List<string>();
            }
            dto.Products = plan.PlanProducts.Select(pp => new SubscriptionPlanProductDto
            {
                ProductId = pp.ProductId,
                ProductName = pp.Product.Name,
                ProductSKU = pp.Product.SKU,
                Quantity = pp.Quantity,
                DiscountPercentage = pp.DiscountPercentage,
                IsOptional = pp.IsOptional
            }).ToList();

            return dto;
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequest request)
        {
            var plan = new SubscriptionPlan
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                BillingInterval = request.BillingInterval,
                BillingIntervalCount = request.BillingIntervalCount,
                Price = request.Price,
                SetupFee = request.SetupFee,
                TrialPeriodDays = request.TrialPeriodDays,
                FeaturesJson = JsonConvert.SerializeObject(request.Features),
                CreatedAt = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Add products to plan
            foreach (var productRequest in request.Products)
            {
                var planProduct = new SubscriptionPlanProduct
                {
                    PlanId = plan.PlanId,
                    ProductId = productRequest.ProductId,
                    Quantity = productRequest.Quantity,
                    DiscountPercentage = productRequest.DiscountPercentage,
                    IsOptional = productRequest.IsOptional
                };
                _context.SubscriptionPlanProducts.Add(planProduct);
            }
            await _context.SaveChangesAsync();

            return (await GetPlanByIdAsync(plan.PlanId))!;
        }

        public async Task<SubscriptionPlanDto> UpdatePlanAsync(int planId, CreateSubscriptionPlanRequest request)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.PlanProducts)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null)
                throw new InvalidOperationException("Plan not found");

            plan.Name = request.Name;
            plan.Code = request.Code;
            plan.Description = request.Description;
            plan.BillingInterval = request.BillingInterval;
            plan.BillingIntervalCount = request.BillingIntervalCount;
            plan.Price = request.Price;
            plan.SetupFee = request.SetupFee;
            plan.TrialPeriodDays = request.TrialPeriodDays;
            plan.FeaturesJson = JsonConvert.SerializeObject(request.Features);
            plan.UpdatedAt = DateTime.UtcNow;

            // Update products
            _context.SubscriptionPlanProducts.RemoveRange(plan.PlanProducts);
            foreach (var productRequest in request.Products)
            {
                var planProduct = new SubscriptionPlanProduct
                {
                    PlanId = plan.PlanId,
                    ProductId = productRequest.ProductId,
                    Quantity = productRequest.Quantity,
                    DiscountPercentage = productRequest.DiscountPercentage,
                    IsOptional = productRequest.IsOptional
                };
                _context.SubscriptionPlanProducts.Add(planProduct);
            }

            await _context.SaveChangesAsync();
            return (await GetPlanByIdAsync(plan.PlanId))!;
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null) return false;

            // Don't allow deletion if there are active subscriptions
            if (plan.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active))
            {
                throw new InvalidOperationException("Cannot delete plan with active subscriptions");
            }

            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TogglePlanStatusAsync(int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return false;

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Subscription Management

        public async Task<SubscriptionDto?> GetSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .Include(s => s.ShippingAddress)
                .Include(s => s.Payments.OrderByDescending(p => p.PaymentDate).Take(5))
                .Include(s => s.Orders.Where(o => o.ScheduledDate > DateTime.UtcNow && !o.IsSkipped).Take(3))
                    .ThenInclude(so => so.Order)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return null;

            return MapSubscriptionToDto(subscription);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(int userId)
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .Include(s => s.ShippingAddress)
                .Include(s => s.Payments.OrderByDescending(p => p.PaymentDate).Take(5))
                .Include(s => s.Orders.Where(o => o.ScheduledDate > DateTime.UtcNow && !o.IsSkipped).Take(3))
                    .ThenInclude(so => so.Order)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return subscriptions.Select(MapSubscriptionToDto);
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
            if (plan == null || !plan.IsActive)
                throw new InvalidOperationException("Plan not found or inactive");

            var subscription = new Subscription
            {
                SubscriptionNumber = GenerateSubscriptionNumber(),
                UserId = request.UserId,
                PlanId = request.PlanId,
                Status = request.StartTrial && plan.TrialPeriodDays.HasValue
                    ? SubscriptionStatus.Trialing
                    : SubscriptionStatus.Active,
                CurrentPrice = plan.Price,
                StartDate = DateTime.UtcNow,
                TrialEndDate = request.StartTrial && plan.TrialPeriodDays.HasValue
                    ? DateTime.UtcNow.AddDays(plan.TrialPeriodDays.Value)
                    : null,
                NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, plan),
                PaymentMethodId = request.PaymentMethodId,
                ShippingAddressId = request.ShippingAddressId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Send welcome email
            await _emailService.SendSubscriptionWelcomeEmailAsync(
                user.Email,
                plan.Name,
                subscription.NextBillingDate.ToString("yyyy-MM-dd"));

            // Create first order if not in trial
            if (subscription.Status == SubscriptionStatus.Active)
            {
                await CreateSubscriptionOrderAsync(subscription.SubscriptionId);
            }

            return (await GetSubscriptionAsync(subscription.SubscriptionId))!;
        }

        public async Task<SubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, UpdateSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            if (request.NewPlanId.HasValue)
            {
                return await ChangePlanAsync(subscriptionId, request.NewPlanId.Value);
            }

            if (request.ShippingAddressId.HasValue)
            {
                subscription.ShippingAddressId = request.ShippingAddressId.Value;
            }

            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                subscription.PaymentMethodId = request.PaymentMethodId;
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (await GetSubscriptionAsync(subscriptionId))!;
        }

        public async Task<bool> PauseSubscriptionAsync(int subscriptionId, PauseSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Paused;
            subscription.PausedUntil = request.PauseUntil;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Record modification
            var modification = new SubscriptionModification
            {
                SubscriptionId = subscriptionId,
                Type = SubscriptionModificationType.Pause,
                EffectiveDate = DateTime.UtcNow,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionModifications.Add(modification);

            await _context.SaveChangesAsync();

            // Send notification email
            await _emailService.SendSubscriptionPausedEmailAsync(
                subscription.User.Email,
                request.PauseUntil.ToString("yyyy-MM-dd"));

            return true;
        }

        public async Task<bool> ResumeSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Active;
            subscription.PausedUntil = null;
            subscription.NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, subscription.Plan);
            subscription.UpdatedAt = DateTime.UtcNow;

            // Record modification
            var modification = new SubscriptionModification
            {
                SubscriptionId = subscriptionId,
                Type = SubscriptionModificationType.Resume,
                EffectiveDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionModifications.Add(modification);

            await _context.SaveChangesAsync();

            // Send notification email
            await _emailService.SendSubscriptionResumedEmailAsync(
                subscription.User.Email,
                subscription.NextBillingDate.ToString("yyyy-MM-dd"));

            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId, CancelSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.CancellationReason = request.Reason;

            if (request.CancelImmediately)
            {
                subscription.EndDate = DateTime.UtcNow;
            }
            else
            {
                subscription.EndDate = subscription.NextBillingDate;
            }

            subscription.UpdatedAt = DateTime.UtcNow;

            // Record modification
            var modification = new SubscriptionModification
            {
                SubscriptionId = subscriptionId,
                Type = SubscriptionModificationType.Cancel,
                EffectiveDate = DateTime.UtcNow,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionModifications.Add(modification);

            await _context.SaveChangesAsync();

            // Send cancellation email
            await _emailService.SendSubscriptionCancelledEmailAsync(
                subscription.User.Email,
                subscription.Plan.Name,
                subscription.EndDate?.ToString("yyyy-MM-dd") ?? "Immediately");

            return true;
        }

        public async Task<bool> ReactivateSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Active;
            subscription.CancelledAt = null;
            subscription.CancellationReason = null;
            subscription.EndDate = null;
            subscription.NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, subscription.Plan);
            subscription.UpdatedAt = DateTime.UtcNow;

            // Record modification
            var modification = new SubscriptionModification
            {
                SubscriptionId = subscriptionId,
                Type = SubscriptionModificationType.Reactivate,
                EffectiveDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionModifications.Add(modification);

            await _context.SaveChangesAsync();

            // Send reactivation email
            await _emailService.SendSubscriptionReactivatedEmailAsync(
                subscription.User.Email,
                subscription.Plan.Name);

            return true;
        }

        #endregion

        #region Subscription Billing

        public async Task<bool> ProcessSubscriptionPaymentAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return false;

            try
            {
                // Process payment through payment service
                var paymentResult = await _paymentService.ProcessSubscriptionPaymentAsync(
                    subscription.PaymentMethodId!,
                    subscription.CurrentPrice);

                var payment = new SubscriptionPayment
                {
                    SubscriptionId = subscriptionId,
                    Amount = subscription.CurrentPrice,
                    Status = paymentResult ? PaymentStatus.Paid : PaymentStatus.Failed,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedDate = paymentResult ? DateTime.UtcNow : null,
                    PeriodStartDate = subscription.NextBillingDate,
                    PeriodEndDate = CalculateNextBillingDate(subscription.NextBillingDate, subscription.Plan),
                    CreatedAt = DateTime.UtcNow
                };

                _context.SubscriptionPayments.Add(payment);

                if (paymentResult)
                {
                    subscription.NextBillingDate = CalculateNextBillingDate(subscription.NextBillingDate, subscription.Plan);
                    await CreateSubscriptionOrderAsync(subscriptionId);
                }
                else
                {
                    subscription.Status = SubscriptionStatus.PastDue;
                    payment.NextRetryDate = DateTime.UtcNow.AddDays(3);
                }

                await _context.SaveChangesAsync();
                return paymentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription payment for {SubscriptionId}", subscriptionId);
                return false;
            }
        }

        public async Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(int subscriptionId)
        {
            var payments = await _context.SubscriptionPayments
                .Where(p => p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SubscriptionPaymentDto>>(payments);
        }

        public async Task<bool> RetryFailedPaymentAsync(int paymentId)
        {
            var payment = await _context.SubscriptionPayments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null || payment.Status != PaymentStatus.Failed)
                return false;

            payment.RetryCount++;

            try
            {
                var paymentResult = await _paymentService.ProcessSubscriptionPaymentAsync(
                    payment.Subscription.PaymentMethodId!,
                    payment.Amount);

                if (paymentResult)
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.ProcessedDate = DateTime.UtcNow;
                    payment.Subscription.Status = SubscriptionStatus.Active;
                    payment.Subscription.NextBillingDate = CalculateNextBillingDate(
                        payment.PeriodEndDate,
                        payment.Subscription.Plan);
                }
                else
                {
                    payment.NextRetryDate = DateTime.UtcNow.AddDays(3 * payment.RetryCount);
                }

                await _context.SaveChangesAsync();
                return paymentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<SubscriptionDto> ChangePlanAsync(int subscriptionId, int newPlanId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
            if (newPlan == null || !newPlan.IsActive)
                throw new InvalidOperationException("New plan not found or inactive");

            // Calculate prorated amount if needed
            var proratedAmount = await CalculateProratedAmountAsync(subscriptionId, newPlanId);

            // Record modification
            var modification = new SubscriptionModification
            {
                SubscriptionId = subscriptionId,
                Type = subscription.Plan.Price > newPlan.Price
                    ? SubscriptionModificationType.Downgrade
                    : SubscriptionModificationType.Upgrade,
                OldPlanId = subscription.PlanId,
                NewPlanId = newPlanId,
                OldPrice = subscription.CurrentPrice,
                NewPrice = newPlan.Price,
                EffectiveDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionModifications.Add(modification);

            // Update subscription
            subscription.PlanId = newPlanId;
            subscription.CurrentPrice = newPlan.Price;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (await GetSubscriptionAsync(subscriptionId))!;
        }

        public async Task<decimal> CalculateProratedAmountAsync(int subscriptionId, int newPlanId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null) return 0;

            var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
            if (newPlan == null) return 0;

            var daysInPeriod = (subscription.NextBillingDate - DateTime.UtcNow).Days;
            var daysRemaining = Math.Max(0, daysInPeriod);

            var oldDailyRate = subscription.CurrentPrice / 30;
            var newDailyRate = newPlan.Price / 30;

            var credit = oldDailyRate * daysRemaining;
            var charge = newDailyRate * daysRemaining;

            return Math.Max(0, charge - credit);
        }

        #endregion

        #region Subscription Orders

        public async Task<SubscriptionOrderDto> CreateSubscriptionOrderAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                    .ThenInclude(p => p.PlanProducts)
                        .ThenInclude(pp => pp.Product)
                .Include(s => s.User)
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            // Create order directly for subscription
            var order = new Order
            {
                OrderNumber = $"SUB-{DateTime.UtcNow:yyyyMMddHHmmss}-{subscription.SubscriptionNumber}",
                UserId = subscription.UserId,
                SubTotal = subscription.CurrentPrice,
                ShippingCost = 0, // Free shipping for subscriptions
                Tax = subscription.CurrentPrice * 0.1m, // 10% tax
                TotalAmount = subscription.CurrentPrice * 1.1m,
                Status = OrderStatus.Processing,
                PaymentStatus = PaymentStatus.Paid,
                OrderDate = DateTime.UtcNow,
                ShippingFirstName = subscription.ShippingAddress?.FirstName ?? subscription.User.FirstName,
                ShippingLastName = subscription.ShippingAddress?.LastName ?? subscription.User.LastName,
                ShippingAddress = subscription.ShippingAddress?.AddressLine1 ?? "",
                ShippingCity = subscription.ShippingAddress?.City ?? "",
                ShippingState = subscription.ShippingAddress?.State ?? "",
                ShippingZipCode = subscription.ShippingAddress?.ZipCode ?? "",
                ShippingCountry = subscription.ShippingAddress?.Country ?? ""
            };

            // Add order items
            foreach (var pp in subscription.Plan.PlanProducts)
            {
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = pp.ProductId,
                    Quantity = pp.Quantity,
                    UnitPrice = pp.Product.Price * (1 - (pp.DiscountPercentage ?? 0) / 100),
                    TotalPrice = pp.Product.Price * pp.Quantity * (1 - (pp.DiscountPercentage ?? 0) / 100)
                };
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var subscriptionOrder = new SubscriptionOrder
            {
                SubscriptionId = subscriptionId,
                OrderId = order.Id,
                ScheduledDate = subscription.NextBillingDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.SubscriptionOrders.Add(subscriptionOrder);
            await _context.SaveChangesAsync();

            return new SubscriptionOrderDto
            {
                SubscriptionOrderId = subscriptionOrder.SubscriptionOrderId,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ScheduledDate = subscriptionOrder.ScheduledDate,
                IsSkipped = false
            };
        }

        public async Task<bool> SkipNextOrderAsync(int subscriptionId, string reason)
        {
            var nextOrder = await _context.SubscriptionOrders
                .Where(so => so.SubscriptionId == subscriptionId &&
                             so.ScheduledDate > DateTime.UtcNow &&
                             !so.IsSkipped)
                .OrderBy(so => so.ScheduledDate)
                .FirstOrDefaultAsync();

            if (nextOrder == null) return false;

            nextOrder.IsSkipped = true;
            nextOrder.SkipReason = reason;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SubscriptionOrderDto>> GetUpcomingOrdersAsync(int subscriptionId)
        {
            var orders = await _context.SubscriptionOrders
                .Include(so => so.Order)
                .Where(so => so.SubscriptionId == subscriptionId &&
                             so.ScheduledDate > DateTime.UtcNow)
                .OrderBy(so => so.ScheduledDate)
                .ToListAsync();

            return orders.Select(so => new SubscriptionOrderDto
            {
                SubscriptionOrderId = so.SubscriptionOrderId,
                OrderId = so.OrderId,
                OrderNumber = so.Order.OrderNumber,
                ScheduledDate = so.ScheduledDate,
                IsSkipped = so.IsSkipped,
                SkipReason = so.SkipReason
            });
        }

        public async Task<bool> UpdateShippingAddressAsync(int subscriptionId, int addressId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null) return false;

            subscription.ShippingAddressId = addressId;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Returns & RMA

        public async Task<ReturnRequestDto> CreateReturnRequestAsync(CreateReturnRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            var returnRequest = new ReturnRequest
            {
                ReturnNumber = GenerateReturnNumber(),
                OrderId = request.OrderId,
                UserId = order.UserId ?? 0,
                Status = ReturnStatus.Pending,
                Reason = request.Reason,
                Comments = request.Comments,
                RequestDate = DateTime.UtcNow
            };

            decimal totalRefundAmount = 0;
            foreach (var item in request.Items)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == item.ProductId);
                if (orderItem == null) continue;

                var refundAmount = orderItem.UnitPrice * item.Quantity;
                totalRefundAmount += refundAmount;

                var returnItem = new ReturnItem
                {
                    Return = returnRequest,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Condition = item.Condition,
                    RefundAmount = refundAmount
                };
                returnRequest.ReturnItems.Add(returnItem);
            }

            returnRequest.RefundAmount = totalRefundAmount;

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            return await GetReturnRequestAsync(returnRequest.ReturnId) ?? throw new InvalidOperationException();
        }

        public async Task<ReturnRequestDto?> GetReturnRequestAsync(int returnId)
        {
            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (returnRequest == null) return null;

            return new ReturnRequestDto
            {
                ReturnId = returnRequest.ReturnId,
                ReturnNumber = returnRequest.ReturnNumber,
                OrderId = returnRequest.OrderId,
                OrderNumber = returnRequest.Order.OrderNumber,
                Status = returnRequest.Status,
                Reason = returnRequest.Reason,
                Comments = returnRequest.Comments,
                RequestDate = returnRequest.RequestDate,
                ApprovedDate = returnRequest.ApprovedDate,
                RefundAmount = returnRequest.RefundAmount,
                RefundMethod = returnRequest.RefundMethod,
                TrackingNumber = returnRequest.TrackingNumber,
                Items = returnRequest.ReturnItems.Select(ri => new ReturnItemDto
                {
                    ProductId = ri.ProductId,
                    ProductName = ri.Product.Name,
                    ProductSKU = ri.Product.SKU,
                    Quantity = ri.Quantity,
                    Condition = ri.Condition,
                    RefundAmount = ri.RefundAmount
                }).ToList()
            };
        }

        public async Task<IEnumerable<ReturnRequestDto>> GetUserReturnsAsync(int userId)
        {
            var returns = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returns.Select(r => new ReturnRequestDto
            {
                ReturnId = r.ReturnId,
                ReturnNumber = r.ReturnNumber,
                OrderId = r.OrderId,
                OrderNumber = r.Order.OrderNumber,
                Status = r.Status,
                Reason = r.Reason,
                Comments = r.Comments,
                RequestDate = r.RequestDate,
                ApprovedDate = r.ApprovedDate,
                RefundAmount = r.RefundAmount,
                RefundMethod = r.RefundMethod,
                TrackingNumber = r.TrackingNumber,
                Items = r.ReturnItems.Select(ri => new ReturnItemDto
                {
                    ProductId = ri.ProductId,
                    ProductName = ri.Product.Name,
                    ProductSKU = ri.Product.SKU,
                    Quantity = ri.Quantity,
                    Condition = ri.Condition,
                    RefundAmount = ri.RefundAmount
                }).ToList()
            });
        }

        public async Task<IEnumerable<ReturnRequestDto>> GetAllReturnsAsync(ReturnStatus? status = null)
        {
            var query = _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var returns = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returns.Select(r => new ReturnRequestDto
            {
                ReturnId = r.ReturnId,
                ReturnNumber = r.ReturnNumber,
                OrderId = r.OrderId,
                OrderNumber = r.Order.OrderNumber,
                Status = r.Status,
                Reason = r.Reason,
                Comments = r.Comments,
                RequestDate = r.RequestDate,
                ApprovedDate = r.ApprovedDate,
                RefundAmount = r.RefundAmount,
                RefundMethod = r.RefundMethod,
                TrackingNumber = r.TrackingNumber,
                Items = r.ReturnItems.Select(ri => new ReturnItemDto
                {
                    ProductId = ri.ProductId,
                    ProductName = ri.Product.Name,
                    ProductSKU = ri.Product.SKU,
                    Quantity = ri.Quantity,
                    Condition = ri.Condition,
                    RefundAmount = ri.RefundAmount
                }).ToList()
            });
        }

        public async Task<ReturnRequestDto> ProcessReturnAsync(int returnId, ProcessReturnRequest request)
        {
            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (returnRequest == null)
                throw new InvalidOperationException("Return request not found");

            if (request.Approve)
            {
                returnRequest.Status = ReturnStatus.Approved;
                returnRequest.ApprovedDate = DateTime.UtcNow;
                returnRequest.RefundMethod = request.RefundMethod;

                if (request.RestockingFee.HasValue)
                {
                    returnRequest.RestockingFee = request.RestockingFee.Value;
                    returnRequest.RefundAmount -= request.RestockingFee.Value;
                }
            }
            else
            {
                returnRequest.Status = ReturnStatus.Rejected;
            }

            returnRequest.Comments = request.Comments;
            await _context.SaveChangesAsync();

            return await GetReturnRequestAsync(returnId) ?? throw new InvalidOperationException();
        }

        public async Task<bool> UpdateReturnTrackingAsync(int returnId, string trackingNumber)
        {
            var returnRequest = await _context.ReturnRequests.FindAsync(returnId);
            if (returnRequest == null) return false;

            returnRequest.TrackingNumber = trackingNumber;
            returnRequest.Status = ReturnStatus.Shipped;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteReturnAsync(int returnId)
        {
            var returnRequest = await _context.ReturnRequests.FindAsync(returnId);
            if (returnRequest == null) return false;

            returnRequest.Status = ReturnStatus.Completed;
            returnRequest.ProcessedDate = DateTime.UtcNow;
            returnRequest.RefundProcessedDate = DateTime.UtcNow;

            // Process refund through payment service
            await _paymentService.ProcessRefundAsync(
                returnRequest.OrderId,
                returnRequest.RefundAmount);

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Abandoned Cart Recovery

        public async Task<AbandonedCartDto> DetectAbandonedCartAsync(int? userId, string? sessionId)
        {
            // Get cart items from session or user
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c =>
                    (userId.HasValue && c.UserId == userId) ||
                    (!string.IsNullOrEmpty(sessionId) && c.SessionId == sessionId));

            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("No cart found");

            var abandonedCart = new AbandonedCart
            {
                UserId = cart.UserId,
                GuestEmail = cart.GuestEmail,
                SessionId = cart.SessionId,
                AbandonedAt = DateTime.UtcNow,
                CartValue = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                ItemCount = cart.CartItems.Sum(ci => ci.Quantity),
                CartItemsJson = JsonConvert.SerializeObject(cart.CartItems.Select(ci => new
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                })),
                RecoveryCode = GenerateRecoveryCode(),
                CreatedAt = DateTime.UtcNow
            };

            _context.AbandonedCarts.Add(abandonedCart);
            await _context.SaveChangesAsync();

            return new AbandonedCartDto
            {
                AbandonedCartId = abandonedCart.AbandonedCartId,
                UserId = abandonedCart.UserId,
                UserEmail = cart.User?.Email,
                GuestEmail = abandonedCart.GuestEmail,
                AbandonedAt = abandonedCart.AbandonedAt,
                CartValue = abandonedCart.CartValue,
                ItemCount = abandonedCart.ItemCount,
                RecoveryCode = abandonedCart.RecoveryCode,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price,
                    TotalPrice = ci.Product.Price * ci.Quantity
                }).ToList()
            };
        }

        public async Task<IEnumerable<AbandonedCartDto>> GetAbandonedCartsAsync(DateTime? since = null)
        {
            var query = _context.AbandonedCarts
                .Include(ac => ac.User)
                .Where(ac => !ac.IsRecovered);

            if (since.HasValue)
            {
                query = query.Where(ac => ac.AbandonedAt >= since.Value);
            }

            var abandonedCarts = await query.ToListAsync();

            return abandonedCarts.Select(ac => new AbandonedCartDto
            {
                AbandonedCartId = ac.AbandonedCartId,
                UserId = ac.UserId,
                UserEmail = ac.User?.Email,
                GuestEmail = ac.GuestEmail,
                AbandonedAt = ac.AbandonedAt,
                CartValue = ac.CartValue,
                ItemCount = ac.ItemCount,
                RecoveryEmailsSent = ac.RecoveryEmailsSent,
                IsRecovered = ac.IsRecovered,
                RecoveryCode = ac.RecoveryCode
            });
        }

        public async Task<bool> SendRecoveryEmailAsync(RecoverAbandonedCartRequest request)
        {
            var abandonedCart = await _context.AbandonedCarts
                .Include(ac => ac.User)
                .FirstOrDefaultAsync(ac => ac.AbandonedCartId == request.AbandonedCartId);

            if (abandonedCart == null) return false;

            var email = abandonedCart.User?.Email ?? abandonedCart.GuestEmail;
            if (string.IsNullOrEmpty(email)) return false;

            // Send recovery email
            await _emailService.SendAbandonedCartRecoveryEmailAsync(
                email,
                abandonedCart.RecoveryCode!,
                abandonedCart.CartValue,
                request.DiscountPercentage);

            abandonedCart.RecoveryEmailsSent++;
            abandonedCart.LastEmailSentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RecoverCartAsync(string recoveryCode)
        {
            var abandonedCart = await _context.AbandonedCarts
                .FirstOrDefaultAsync(ac => ac.RecoveryCode == recoveryCode && !ac.IsRecovered);

            if (abandonedCart == null) return false;

            // Restore cart items
            if (!string.IsNullOrEmpty(abandonedCart.CartItemsJson))
            {
                var items = JsonConvert.DeserializeObject<List<dynamic>>(abandonedCart.CartItemsJson);
                // Restore cart logic would go here
            }

            return true;
        }

        public async Task<bool> MarkCartAsRecoveredAsync(int abandonedCartId, int orderId)
        {
            var abandonedCart = await _context.AbandonedCarts.FindAsync(abandonedCartId);
            if (abandonedCart == null) return false;

            abandonedCart.IsRecovered = true;
            abandonedCart.RecoveredAt = DateTime.UtcNow;
            abandonedCart.RecoveredOrderId = orderId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateRecoveryRateAsync(DateTime startDate, DateTime endDate)
        {
            var totalAbandoned = await _context.AbandonedCarts
                .CountAsync(ac => ac.AbandonedAt >= startDate && ac.AbandonedAt <= endDate);

            var totalRecovered = await _context.AbandonedCarts
                .CountAsync(ac => ac.AbandonedAt >= startDate &&
                                  ac.AbandonedAt <= endDate &&
                                  ac.IsRecovered);

            if (totalAbandoned == 0) return 0;

            return (decimal)totalRecovered / totalAbandoned * 100;
        }

        #endregion

        #region Gift Cards

        public async Task<GiftCardDto> CreateGiftCardAsync(CreateGiftCardRequest request)
        {
            var giftCard = new GiftCard
            {
                Code = GenerateGiftCardCode(),
                InitialValue = request.Value,
                CurrentBalance = request.Value,
                RecipientEmail = request.RecipientEmail,
                RecipientName = request.RecipientName,
                Message = request.Message,
                ExpiresAt = request.ExpiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.GiftCards.Add(giftCard);

            // Create initial transaction
            var transaction = new GiftCardTransaction
            {
                GiftCard = giftCard,
                Type = GiftCardTransactionType.Purchase,
                Amount = request.Value,
                BalanceBefore = 0,
                BalanceAfter = request.Value,
                Description = "Gift card created",
                TransactionDate = DateTime.UtcNow
            };
            _context.GiftCardTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            // Send gift card email
            if (!string.IsNullOrEmpty(request.RecipientEmail))
            {
                await _emailService.SendGiftCardEmailAsync(
                    request.RecipientEmail,
                    giftCard.Code,
                    request.Value,
                    request.Message);
            }

            return new GiftCardDto
            {
                GiftCardId = giftCard.GiftCardId,
                Code = giftCard.Code,
                InitialValue = giftCard.InitialValue,
                CurrentBalance = giftCard.CurrentBalance,
                RecipientEmail = giftCard.RecipientEmail,
                RecipientName = giftCard.RecipientName,
                Message = giftCard.Message,
                IsActive = giftCard.IsActive,
                ExpiresAt = giftCard.ExpiresAt
            };
        }

        public async Task<GiftCardDto?> GetGiftCardAsync(string code)
        {
            var giftCard = await _context.GiftCards
                .Include(gc => gc.Transactions)
                .FirstOrDefaultAsync(gc => gc.Code == code);

            if (giftCard == null) return null;

            return new GiftCardDto
            {
                GiftCardId = giftCard.GiftCardId,
                Code = giftCard.Code,
                InitialValue = giftCard.InitialValue,
                CurrentBalance = giftCard.CurrentBalance,
                RecipientEmail = giftCard.RecipientEmail,
                RecipientName = giftCard.RecipientName,
                Message = giftCard.Message,
                IsActive = giftCard.IsActive,
                ExpiresAt = giftCard.ExpiresAt,
                Transactions = giftCard.Transactions.Select(t => new GiftCardTransactionDto
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate
                }).ToList()
            };
        }

        public async Task<decimal> GetGiftCardBalanceAsync(string code)
        {
            var giftCard = await _context.GiftCards
                .FirstOrDefaultAsync(gc => gc.Code == code && gc.IsActive);

            return giftCard?.CurrentBalance ?? 0;
        }

        public async Task<bool> RedeemGiftCardAsync(RedeemGiftCardRequest request)
        {
            var giftCard = await _context.GiftCards
                .FirstOrDefaultAsync(gc => gc.Code == request.Code && gc.IsActive);

            if (giftCard == null || giftCard.CurrentBalance <= 0)
                return false;

            if (giftCard.ExpiresAt.HasValue && giftCard.ExpiresAt < DateTime.UtcNow)
                return false;

            // Apply to order if specified
            if (request.OrderId.HasValue)
            {
                var order = await _context.Orders.FindAsync(request.OrderId.Value);
                if (order == null) return false;

                var amountToUse = Math.Min(giftCard.CurrentBalance, order.TotalAmount);

                var transaction = new GiftCardTransaction
                {
                    GiftCardId = giftCard.GiftCardId,
                    Type = GiftCardTransactionType.Redemption,
                    Amount = amountToUse,
                    BalanceBefore = giftCard.CurrentBalance,
                    BalanceAfter = giftCard.CurrentBalance - amountToUse,
                    OrderId = request.OrderId,
                    Description = $"Applied to order {order.OrderNumber}",
                    TransactionDate = DateTime.UtcNow
                };
                _context.GiftCardTransactions.Add(transaction);

                giftCard.CurrentBalance -= amountToUse;
                giftCard.UpdatedAt = DateTime.UtcNow;

                if (giftCard.CurrentBalance == 0)
                {
                    giftCard.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReloadGiftCardAsync(string code, decimal amount)
        {
            var giftCard = await _context.GiftCards
                .FirstOrDefaultAsync(gc => gc.Code == code);

            if (giftCard == null) return false;

            var transaction = new GiftCardTransaction
            {
                GiftCardId = giftCard.GiftCardId,
                Type = GiftCardTransactionType.Adjustment,
                Amount = amount,
                BalanceBefore = giftCard.CurrentBalance,
                BalanceAfter = giftCard.CurrentBalance + amount,
                Description = "Gift card reloaded",
                TransactionDate = DateTime.UtcNow
            };
            _context.GiftCardTransactions.Add(transaction);

            giftCard.CurrentBalance += amount;
            giftCard.IsActive = true;
            giftCard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(int userId)
        {
            var giftCards = await _context.GiftCards
                .Where(gc => gc.PurchasedByUserId == userId || gc.RecipientUserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GiftCardDto>>(giftCards);
        }

        public async Task<IEnumerable<GiftCardTransactionDto>> GetGiftCardTransactionsAsync(string code)
        {
            var transactions = await _context.GiftCardTransactions
                .Include(t => t.GiftCard)
                .Where(t => t.GiftCard.Code == code)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GiftCardTransactionDto>>(transactions);
        }

        public async Task<bool> DeactivateGiftCardAsync(string code)
        {
            var giftCard = await _context.GiftCards
                .FirstOrDefaultAsync(gc => gc.Code == code);

            if (giftCard == null) return false;

            giftCard.IsActive = false;
            giftCard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsGiftCardOwnerAsync(string code, int userId)
        {
            var giftCard = await _context.GiftCards
                .FirstOrDefaultAsync(gc => gc.Code == code);

            if (giftCard == null) return false;

            // Check if user is the purchaser or the recipient
            return giftCard.PurchasedByUserId == userId || giftCard.RecipientUserId == userId;
        }

        #endregion

        #region Analytics

        public async Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            var analytics = new SalesAnalyticsDto
            {
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                OrdersByStatus = orders.GroupBy(o => o.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                RevenueByCategory = orders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.Product.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(oi => oi.UnitPrice * oi.Quantity))
            };

            // Calculate daily sales
            analytics.DailySales = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                    Items = g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity))
                })
                .OrderBy(ds => ds.Date)
                .ToList();

            return analytics;
        }

        public async Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var customers = await _context.Users
                .Include(u => u.Orders)
                .ToListAsync();

            var newCustomers = customers.Count(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);
            var returningCustomers = customers.Count(c => c.Orders.Count(o => o.OrderDate >= startDate && o.OrderDate <= endDate) > 1);

            var analytics = new CustomerAnalyticsDto
            {
                TotalCustomers = customers.Count,
                NewCustomers = newCustomers,
                ReturningCustomers = returningCustomers,
                CustomerLifetimeValue = await CalculateCustomerLifetimeValueAsync()
            };

            // Calculate customer segments
            analytics.Segments = new List<CustomerSegmentDto>
            {
                new CustomerSegmentDto
                {
                    SegmentName = "VIP Customers",
                    CustomerCount = customers.Count(c => c.Orders.Sum(o => o.TotalAmount) > 1000),
                    AverageOrderValue = customers
                        .Where(c => c.Orders.Sum(o => o.TotalAmount) > 1000)
                        .SelectMany(c => c.Orders)
                        .DefaultIfEmpty()
                        .Average(o => o?.TotalAmount ?? 0)
                },
                new CustomerSegmentDto
                {
                    SegmentName = "Regular Customers",
                    CustomerCount = customers.Count(c => c.Orders.Count > 3),
                    AverageOrderValue = customers
                        .Where(c => c.Orders.Count > 3)
                        .SelectMany(c => c.Orders)
                        .DefaultIfEmpty()
                        .Average(o => o?.TotalAmount ?? 0)
                },
                new CustomerSegmentDto
                {
                    SegmentName = "New Customers",
                    CustomerCount = customers.Count(c => c.Orders.Count == 1),
                    AverageOrderValue = customers
                        .Where(c => c.Orders.Count == 1)
                        .SelectMany(c => c.Orders)
                        .DefaultIfEmpty()
                        .Average(o => o?.TotalAmount ?? 0)
                }
            };

            return analytics;
        }

        public async Task<ProductAnalyticsDto> GetProductAnalyticsAsync(DateTime startDate, DateTime endDate, int? categoryId = null)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToListAsync();

            if (categoryId.HasValue)
            {
                orderItems = orderItems.Where(oi => oi.Product.CategoryId == categoryId).ToList();
            }

            var productSales = orderItems
                .GroupBy(oi => oi.Product)
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .ToList();

            var analytics = new ProductAnalyticsDto
            {
                TopSellingProducts = productSales.Take(10).ToList(),
                LowPerformingProducts = productSales.TakeLast(10).ToList()
            };

            return analytics;
        }

        public async Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync()
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .ToListAsync();

            var activeSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
            var mrr = await CalculateMonthlyRecurringRevenueAsync();

            var analytics = new SubscriptionAnalyticsDto
            {
                ActiveSubscriptions = activeSubscriptions,
                MonthlyRecurringRevenue = mrr,
                AnnualRecurringRevenue = mrr * 12,
                AverageSubscriptionValue = activeSubscriptions > 0
                    ? subscriptions.Where(s => s.Status == SubscriptionStatus.Active).Average(s => s.CurrentPrice)
                    : 0,
                SubscriptionsByPlan = subscriptions
                    .GroupBy(s => s.Plan.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Calculate growth trend
            analytics.GrowthTrend = new List<SubscriptionGrowthDto>();
            for (int i = 11; i >= 0; i--)
            {
                var month = DateTime.UtcNow.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var newSubs = subscriptions.Count(s => s.CreatedAt >= monthStart && s.CreatedAt <= monthEnd);
                var cancellations = subscriptions.Count(s => s.CancelledAt >= monthStart && s.CancelledAt <= monthEnd);

                analytics.GrowthTrend.Add(new SubscriptionGrowthDto
                {
                    Month = monthStart,
                    NewSubscriptions = newSubs,
                    Cancellations = cancellations,
                    NetGrowth = newSubs - cancellations
                });
            }

            return analytics;
        }

        public async Task<decimal> CalculateMonthlyRecurringRevenueAsync()
        {
            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .ToListAsync();

            return activeSubscriptions.Sum(s => s.CurrentPrice);
        }

        public async Task<decimal> CalculateChurnRateAsync(DateTime startDate, DateTime endDate)
        {
            var startSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.CreatedAt < startDate &&
                                 (s.CancelledAt == null || s.CancelledAt > startDate));

            var cancelledSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.CancelledAt >= startDate && s.CancelledAt <= endDate);

            if (startSubscriptions == 0) return 0;

            return (decimal)cancelledSubscriptions / startSubscriptions * 100;
        }

        public async Task<decimal> CalculateCustomerLifetimeValueAsync(int? userId = null)
        {
            IQueryable<User> query = _context.Users.Include(u => u.Orders);

            if (userId.HasValue)
            {
                query = query.Where(u => u.Id == userId.Value);
            }

            var users = await query.ToListAsync();

            if (!users.Any()) return 0;

            var totalRevenue = users.Sum(u => u.Orders.Sum(o => o.TotalAmount));
            var totalCustomers = users.Count;

            return totalCustomers > 0 ? totalRevenue / totalCustomers : 0;
        }

        #endregion

        #region Batch Operations

        public async Task<int> ProcessDueSubscriptionsAsync()
        {
            var dueSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate <= DateTime.UtcNow)
                .ToListAsync();

            int processed = 0;
            foreach (var subscription in dueSubscriptions)
            {
                if (await ProcessSubscriptionPaymentAsync(subscription.SubscriptionId))
                {
                    processed++;
                }
            }

            return processed;
        }

        public async Task<int> SendPaymentRemindersAsync()
        {
            var pastDueSubscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Where(s => s.Status == SubscriptionStatus.PastDue)
                .ToListAsync();

            int sent = 0;
            foreach (var subscription in pastDueSubscriptions)
            {
                await _emailService.SendPaymentReminderEmailAsync(
                    subscription.User.Email,
                    subscription.CurrentPrice);
                sent++;
            }

            return sent;
        }

        public async Task<int> ProcessAbandonedCartsAsync()
        {
            var abandonedCarts = await _context.AbandonedCarts
                .Where(ac => !ac.IsRecovered &&
                            ac.AbandonedAt < DateTime.UtcNow.AddHours(-24) &&
                            ac.RecoveryEmailsSent < 3)
                .ToListAsync();

            int processed = 0;
            foreach (var cart in abandonedCarts)
            {
                await SendRecoveryEmailAsync(new RecoverAbandonedCartRequest
                {
                    AbandonedCartId = cart.AbandonedCartId,
                    DiscountPercentage = 10
                });
                processed++;
            }

            return processed;
        }

        public async Task<int> ExpireGiftCardsAsync()
        {
            var expiredCards = await _context.GiftCards
                .Where(gc => gc.IsActive &&
                            gc.ExpiresAt.HasValue &&
                            gc.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var card in expiredCards)
            {
                card.IsActive = false;

                var transaction = new GiftCardTransaction
                {
                    GiftCardId = card.GiftCardId,
                    Type = GiftCardTransactionType.Expiration,
                    Amount = card.CurrentBalance,
                    BalanceBefore = card.CurrentBalance,
                    BalanceAfter = 0,
                    Description = "Gift card expired",
                    TransactionDate = DateTime.UtcNow
                };
                _context.GiftCardTransactions.Add(transaction);

                card.CurrentBalance = 0;
            }

            await _context.SaveChangesAsync();
            return expiredCards.Count;
        }

        #endregion

        #region Trial Management

        public async Task<bool> StartTrialAsync(int userId, int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null || !plan.TrialPeriodDays.HasValue)
                return false;

            var subscription = new Subscription
            {
                SubscriptionNumber = GenerateSubscriptionNumber(),
                UserId = userId,
                PlanId = planId,
                Status = SubscriptionStatus.Trialing,
                CurrentPrice = plan.Price,
                StartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(plan.TrialPeriodDays.Value),
                NextBillingDate = DateTime.UtcNow.AddDays(plan.TrialPeriodDays.Value),
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndTrialAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || subscription.Status != SubscriptionStatus.Trialing)
                return false;

            subscription.Status = SubscriptionStatus.Active;
            subscription.TrialEndDate = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExtendTrialAsync(int subscriptionId, int days)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || subscription.Status != SubscriptionStatus.Trialing)
                return false;

            subscription.TrialEndDate = subscription.TrialEndDate?.AddDays(days);
            subscription.NextBillingDate = subscription.NextBillingDate.AddDays(days);
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Gift Subscriptions

        public async Task<SubscriptionDto> CreateGiftSubscriptionAsync(int giverUserId, int recipientUserId, int planId, string? message)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                throw new InvalidOperationException("Plan not found");

            var subscription = new Subscription
            {
                SubscriptionNumber = GenerateSubscriptionNumber(),
                UserId = recipientUserId,
                PlanId = planId,
                Status = SubscriptionStatus.Active,
                CurrentPrice = plan.Price,
                StartDate = DateTime.UtcNow,
                NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, plan),
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var giftSubscription = new GiftSubscription
            {
                SubscriptionId = subscription.SubscriptionId,
                GiverUserId = giverUserId,
                RecipientUserId = recipientUserId,
                GiftMessage = message,
                RedemptionCode = GenerateRedemptionCode(),
                CreatedAt = DateTime.UtcNow
            };

            _context.GiftSubscriptions.Add(giftSubscription);
            await _context.SaveChangesAsync();

            return (await GetSubscriptionAsync(subscription.SubscriptionId))!;
        }

        public async Task<bool> RedeemGiftSubscriptionAsync(string redemptionCode, int userId)
        {
            var giftSubscription = await _context.GiftSubscriptions
                .Include(gs => gs.Subscription)
                .FirstOrDefaultAsync(gs => gs.RedemptionCode == redemptionCode && !gs.IsRedeemed);

            if (giftSubscription == null)
                return false;

            giftSubscription.IsRedeemed = true;
            giftSubscription.RedemptionDate = DateTime.UtcNow;
            giftSubscription.Subscription.UserId = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Helper Methods

        private SubscriptionDto MapSubscriptionToDto(Subscription subscription)
        {
            return new SubscriptionDto
            {
                SubscriptionId = subscription.SubscriptionId,
                SubscriptionNumber = subscription.SubscriptionNumber,
                UserId = subscription.UserId,
                UserEmail = subscription.User.Email,
                PlanId = subscription.PlanId,
                PlanName = subscription.Plan.Name,
                Status = subscription.Status,
                CurrentPrice = subscription.CurrentPrice,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                TrialEndDate = subscription.TrialEndDate,
                NextBillingDate = subscription.NextBillingDate,
                PausedUntil = subscription.PausedUntil,
                CancelledAt = subscription.CancelledAt,
                CardLast4 = subscription.CardLast4,
                CardBrand = subscription.CardBrand,
                ShippingAddress = subscription.ShippingAddress != null ? _mapper.Map<AddressDto>(subscription.ShippingAddress) : null,
                RecentPayments = subscription.Payments.Select(p => new SubscriptionPaymentDto
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    PeriodStartDate = p.PeriodStartDate,
                    PeriodEndDate = p.PeriodEndDate,
                    FailureReason = p.FailureReason
                }).ToList(),
                UpcomingOrders = subscription.Orders.Select(o => new SubscriptionOrderDto
                {
                    SubscriptionOrderId = o.SubscriptionOrderId,
                    OrderId = o.OrderId,
                    OrderNumber = o.Order.OrderNumber,
                    ScheduledDate = o.ScheduledDate,
                    IsSkipped = o.IsSkipped,
                    SkipReason = o.SkipReason
                }).ToList()
            };
        }

        private DateTime CalculateNextBillingDate(DateTime fromDate, SubscriptionPlan plan)
        {
            return plan.BillingInterval switch
            {
                SubscriptionInterval.Daily => fromDate.AddDays(plan.BillingIntervalCount),
                SubscriptionInterval.Weekly => fromDate.AddDays(7 * plan.BillingIntervalCount),
                SubscriptionInterval.Monthly => fromDate.AddMonths(plan.BillingIntervalCount),
                SubscriptionInterval.Quarterly => fromDate.AddMonths(3 * plan.BillingIntervalCount),
                SubscriptionInterval.Yearly => fromDate.AddYears(plan.BillingIntervalCount),
                _ => fromDate.AddMonths(1)
            };
        }

        private string GenerateSubscriptionNumber()
        {
            return $"SUB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GenerateReturnNumber()
        {
            return $"RMA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GenerateRecoveryCode()
        {
            return $"RECOVER-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GenerateGiftCardCode()
        {
            return $"GIFT-{Guid.NewGuid().ToString().Substring(0, 12).ToUpper()}";
        }

        private string GenerateRedemptionCode()
        {
            return $"REDEEM-{Guid.NewGuid().ToString().Substring(0, 10).ToUpper()}";
        }

        #endregion
    }
}