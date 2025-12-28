using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs
{
    // Subscription Plan DTOs
    public class SubscriptionPlanDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SubscriptionInterval BillingInterval { get; set; }
        public int BillingIntervalCount { get; set; }
        public decimal Price { get; set; }
        public decimal? SetupFee { get; set; }
        public int? TrialPeriodDays { get; set; }
        public bool IsActive { get; set; }
        public List<string> Features { get; set; } = new();
        public List<SubscriptionPlanProductDto> Products { get; set; } = new();
    }

    public class SubscriptionPlanProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool IsOptional { get; set; }
    }

    public class CreateSubscriptionPlanRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public SubscriptionInterval BillingInterval { get; set; }

        public int BillingIntervalCount { get; set; } = 1;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? SetupFee { get; set; }

        public int? TrialPeriodDays { get; set; }

        public List<string> Features { get; set; } = new();

        public List<AddProductToPlanRequest> Products { get; set; } = new();
    }

    public class AddProductToPlanRequest
    {
        [Required]
        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal? DiscountPercentage { get; set; }

        public bool IsOptional { get; set; } = false;
    }

    // Subscription DTOs
    public class SubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public DateTime? PausedUntil { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public List<SubscriptionPaymentDto> RecentPayments { get; set; } = new();
        public List<SubscriptionOrderDto> UpcomingOrders { get; set; } = new();
    }

    public class CreateSubscriptionRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public string? PaymentMethodId { get; set; }

        public int? ShippingAddressId { get; set; }

        public bool StartTrial { get; set; } = true;
    }

    public class UpdateSubscriptionRequest
    {
        public int? NewPlanId { get; set; }
        public int? ShippingAddressId { get; set; }
        public string? PaymentMethodId { get; set; }
    }

    public class PauseSubscriptionRequest
    {
        [Required]
        public DateTime PauseUntil { get; set; }

        public string? Reason { get; set; }
    }

    public class CancelSubscriptionRequest
    {
        [Required]
        public bool CancelImmediately { get; set; }

        public string? Reason { get; set; }
    }

    public class SubscriptionPaymentDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string? FailureReason { get; set; }
    }

    public class SubscriptionOrderDto
    {
        public int SubscriptionOrderId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public bool IsSkipped { get; set; }
        public string? SkipReason { get; set; }
    }

    // Returns & RMA DTOs
    public class ReturnRequestDto
    {
        public int ReturnId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public ReturnStatus Status { get; set; }
        public ReturnReason Reason { get; set; }
        public string? Comments { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public decimal RefundAmount { get; set; }
        public RefundMethod RefundMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public List<ReturnItemDto> Items { get; set; } = new();
    }

    public class ReturnItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public ReturnItemCondition Condition { get; set; }
        public decimal RefundAmount { get; set; }
    }

    public class CreateReturnRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public ReturnReason Reason { get; set; }

        public string? Comments { get; set; }

        [Required]
        public List<ReturnItemRequest> Items { get; set; } = new();
    }

    public class ReturnItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public ReturnItemCondition Condition { get; set; }
    }

    public class ProcessReturnRequest
    {
        [Required]
        public bool Approve { get; set; }

        public string? Comments { get; set; }

        public decimal? RestockingFee { get; set; }

        public RefundMethod RefundMethod { get; set; } = RefundMethod.OriginalPayment;
    }

    // Abandoned Cart DTOs
    public class AbandonedCartDto
    {
        public int AbandonedCartId { get; set; }
        public int? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? GuestEmail { get; set; }
        public DateTime AbandonedAt { get; set; }
        public decimal CartValue { get; set; }
        public int ItemCount { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int RecoveryEmailsSent { get; set; }
        public bool IsRecovered { get; set; }
        public string? RecoveryCode { get; set; }
    }

    public class RecoverAbandonedCartRequest
    {
        [Required]
        public int AbandonedCartId { get; set; }

        public string? EmailTemplate { get; set; }

        public decimal? DiscountPercentage { get; set; }
    }

    // Gift Card DTOs
    public class GiftCardDto
    {
        public int GiftCardId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal InitialValue { get; set; }
        public decimal CurrentBalance { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientName { get; set; }
        public string? Message { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<GiftCardTransactionDto> Transactions { get; set; } = new();
    }

    public class GiftCardTransactionDto
    {
        public int TransactionId { get; set; }
        public GiftCardTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class CreateGiftCardRequest
    {
        [Required]
        [Range(1, 1000)]
        public decimal Value { get; set; }

        [EmailAddress]
        public string? RecipientEmail { get; set; }

        public string? RecipientName { get; set; }

        public string? Message { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    public class RedeemGiftCardRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        public int? OrderId { get; set; }
    }

    public class GiftCardBalanceRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    // Analytics DTOs
    public class SalesAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<DailySalesDto> DailySales { get; set; } = new();
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int Items { get; set; }
    }

    public class CustomerAnalyticsDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public decimal CustomerAcquisitionCost { get; set; }
        public decimal ChurnRate { get; set; }
        public List<CustomerSegmentDto> Segments { get; set; } = new();
    }

    public class CustomerSegmentDto
    {
        public string SegmentName { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductAnalyticsDto
    {
        public List<TopProductDto> TopSellingProducts { get; set; } = new();
        public List<TopProductDto> LowPerformingProducts { get; set; } = new();
        public Dictionary<int, int> ProductViews { get; set; } = new();
        public Dictionary<int, double> ConversionRates { get; set; } = new();
    }

    public class SubscriptionAnalyticsDto
    {
        public int ActiveSubscriptions { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal AnnualRecurringRevenue { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal AverageSubscriptionValue { get; set; }
        public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
        public List<SubscriptionGrowthDto> GrowthTrend { get; set; } = new();
    }

    public class SubscriptionGrowthDto
    {
        public DateTime Month { get; set; }
        public int NewSubscriptions { get; set; }
        public int Cancellations { get; set; }
        public int NetGrowth { get; set; }
    }
}