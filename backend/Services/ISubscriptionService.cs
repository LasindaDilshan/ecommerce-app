using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface ISubscriptionService
    {
        // Subscription Plan Management
        Task<IEnumerable<SubscriptionPlanDto>> GetAllPlansAsync(bool activeOnly = true);
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId);
        Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequest request);
        Task<SubscriptionPlanDto> UpdatePlanAsync(int planId, CreateSubscriptionPlanRequest request);
        Task<bool> DeletePlanAsync(int planId);
        Task<bool> TogglePlanStatusAsync(int planId);

        // Subscription Management
        Task<SubscriptionDto?> GetSubscriptionAsync(int subscriptionId);
        Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(int userId);
        Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request);
        Task<SubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, UpdateSubscriptionRequest request);
        Task<bool> PauseSubscriptionAsync(int subscriptionId, PauseSubscriptionRequest request);
        Task<bool> ResumeSubscriptionAsync(int subscriptionId);
        Task<bool> CancelSubscriptionAsync(int subscriptionId, CancelSubscriptionRequest request);
        Task<bool> ReactivateSubscriptionAsync(int subscriptionId);

        // Subscription Billing
        Task<bool> ProcessSubscriptionPaymentAsync(int subscriptionId);
        Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(int subscriptionId);
        Task<bool> RetryFailedPaymentAsync(int paymentId);
        Task<SubscriptionDto> ChangePlanAsync(int subscriptionId, int newPlanId);
        Task<decimal> CalculateProratedAmountAsync(int subscriptionId, int newPlanId);

        // Subscription Orders
        Task<SubscriptionOrderDto> CreateSubscriptionOrderAsync(int subscriptionId);
        Task<bool> SkipNextOrderAsync(int subscriptionId, string reason);
        Task<IEnumerable<SubscriptionOrderDto>> GetUpcomingOrdersAsync(int subscriptionId);
        Task<bool> UpdateShippingAddressAsync(int subscriptionId, int addressId);

        // Trial Management
        Task<bool> StartTrialAsync(int userId, int planId);
        Task<bool> EndTrialAsync(int subscriptionId);
        Task<bool> ExtendTrialAsync(int subscriptionId, int days);

        // Gift Subscriptions
        Task<SubscriptionDto> CreateGiftSubscriptionAsync(int giverUserId, int recipientUserId, int planId, string? message);
        Task<bool> RedeemGiftSubscriptionAsync(string redemptionCode, int userId);

        // Returns & RMA
        Task<ReturnRequestDto> CreateReturnRequestAsync(CreateReturnRequest request);
        Task<ReturnRequestDto?> GetReturnRequestAsync(int returnId);
        Task<IEnumerable<ReturnRequestDto>> GetUserReturnsAsync(int userId);
        Task<IEnumerable<ReturnRequestDto>> GetAllReturnsAsync(ReturnStatus? status = null);
        Task<ReturnRequestDto> ProcessReturnAsync(int returnId, ProcessReturnRequest request);
        Task<bool> UpdateReturnTrackingAsync(int returnId, string trackingNumber);
        Task<bool> CompleteReturnAsync(int returnId);

        // Abandoned Cart Recovery
        Task<AbandonedCartDto> DetectAbandonedCartAsync(int? userId, string? sessionId);
        Task<IEnumerable<AbandonedCartDto>> GetAbandonedCartsAsync(DateTime? since = null);
        Task<bool> SendRecoveryEmailAsync(RecoverAbandonedCartRequest request);
        Task<bool> RecoverCartAsync(string recoveryCode);
        Task<bool> MarkCartAsRecoveredAsync(int abandonedCartId, int orderId);
        Task<decimal> CalculateRecoveryRateAsync(DateTime startDate, DateTime endDate);

        // Gift Cards
        Task<GiftCardDto> CreateGiftCardAsync(CreateGiftCardRequest request);
        Task<GiftCardDto?> GetGiftCardAsync(string code);
        Task<decimal> GetGiftCardBalanceAsync(string code);
        Task<bool> RedeemGiftCardAsync(RedeemGiftCardRequest request);
        Task<bool> ReloadGiftCardAsync(string code, decimal amount);
        Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(int userId);
        Task<IEnumerable<GiftCardTransactionDto>> GetGiftCardTransactionsAsync(string code);
        Task<bool> DeactivateGiftCardAsync(string code);
        Task<bool> IsGiftCardOwnerAsync(string code, int userId);

        // Analytics
        Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<ProductAnalyticsDto> GetProductAnalyticsAsync(DateTime startDate, DateTime endDate, int? categoryId = null);
        Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync();
        Task<decimal> CalculateMonthlyRecurringRevenueAsync();
        Task<decimal> CalculateChurnRateAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateCustomerLifetimeValueAsync(int? userId = null);

        // Batch Operations
        Task<int> ProcessDueSubscriptionsAsync();
        Task<int> SendPaymentRemindersAsync();
        Task<int> ProcessAbandonedCartsAsync();
        Task<int> ExpireGiftCardsAsync();
    }
}