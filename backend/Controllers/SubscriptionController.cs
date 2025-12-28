using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        private bool CanAccessUserData(int requestedUserId)
        {
            return IsAdmin() || GetUserId() == requestedUserId;
        }

        private async Task<bool> CanAccessGiftCardAsync(string code)
        {
            if (IsAdmin()) return true;

            var userId = GetUserId();
            return await _subscriptionService.IsGiftCardOwnerAsync(code, userId);
        }

        #region Subscription Plans

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans([FromQuery] bool activeOnly = true)
        {
            var plans = await _subscriptionService.GetAllPlansAsync(activeOnly);
            return Ok(plans);
        }

        [HttpGet("plans/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubscriptionPlanDto>> GetPlan(int id)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(id);
            if (plan == null)
                return NotFound();
            return Ok(plan);
        }

        [HttpPost("plans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(CreateSubscriptionPlanRequest request)
        {
            var plan = await _subscriptionService.CreatePlanAsync(request);
            return CreatedAtAction(nameof(GetPlan), new { id = plan.PlanId }, plan);
        }

        [HttpPut("plans/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPlanDto>> UpdatePlan(int id, CreateSubscriptionPlanRequest request)
        {
            try
            {
                var plan = await _subscriptionService.UpdatePlanAsync(id, request);
                return Ok(plan);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("plans/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            try
            {
                var result = await _subscriptionService.DeletePlanAsync(id);
                if (!result)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("plans/{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePlanStatus(int id)
        {
            var result = await _subscriptionService.TogglePlanStatusAsync(id);
            if (!result)
                return NotFound();
            return Ok();
        }

        #endregion

        #region Subscriptions

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int id)
        {
            var subscription = await _subscriptionService.GetSubscriptionAsync(id);
            if (subscription == null)
                return NotFound();
            return Ok(subscription);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetUserSubscriptions(int userId)
        {
            // Security: Users can only view their own subscriptions unless Admin
            if (!CanAccessUserData(userId))
            {
                return Forbid();
            }
            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionRequest request)
        {
            try
            {
                var subscription = await _subscriptionService.CreateSubscriptionAsync(request);
                return CreatedAtAction(nameof(GetSubscription), new { id = subscription.SubscriptionId }, subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionDto>> UpdateSubscription(int id, UpdateSubscriptionRequest request)
        {
            try
            {
                var subscription = await _subscriptionService.UpdateSubscriptionAsync(id, request);
                return Ok(subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/pause")]
        public async Task<IActionResult> PauseSubscription(int id, PauseSubscriptionRequest request)
        {
            var result = await _subscriptionService.PauseSubscriptionAsync(id, request);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> ResumeSubscription(int id)
        {
            var result = await _subscriptionService.ResumeSubscriptionAsync(id);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(int id, CancelSubscriptionRequest request)
        {
            var result = await _subscriptionService.CancelSubscriptionAsync(id, request);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateSubscription(int id)
        {
            var result = await _subscriptionService.ReactivateSubscriptionAsync(id);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpPost("{id}/change-plan")]
        public async Task<ActionResult<SubscriptionDto>> ChangePlan(int id, [FromBody] int newPlanId)
        {
            try
            {
                var subscription = await _subscriptionService.ChangePlanAsync(id, newPlanId);
                return Ok(subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/calculate-proration")]
        public async Task<ActionResult<decimal>> CalculateProration(int id, [FromQuery] int newPlanId)
        {
            var amount = await _subscriptionService.CalculateProratedAmountAsync(id, newPlanId);
            return Ok(amount);
        }

        #endregion

        #region Subscription Billing

        [HttpPost("{id}/process-payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var result = await _subscriptionService.ProcessSubscriptionPaymentAsync(id);
            if (!result)
                return BadRequest("Payment processing failed");
            return Ok();
        }

        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<SubscriptionPaymentDto>>> GetPayments(int id)
        {
            var payments = await _subscriptionService.GetSubscriptionPaymentsAsync(id);
            return Ok(payments);
        }

        [HttpPost("payments/{paymentId}/retry")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RetryPayment(int paymentId)
        {
            var result = await _subscriptionService.RetryFailedPaymentAsync(paymentId);
            if (!result)
                return BadRequest("Payment retry failed");
            return Ok();
        }

        #endregion

        #region Subscription Orders

        [HttpPost("{id}/create-order")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionOrderDto>> CreateOrder(int id)
        {
            try
            {
                var order = await _subscriptionService.CreateSubscriptionOrderAsync(id);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/skip-next-order")]
        public async Task<IActionResult> SkipNextOrder(int id, [FromBody] string reason)
        {
            var result = await _subscriptionService.SkipNextOrderAsync(id, reason);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpGet("{id}/upcoming-orders")]
        public async Task<ActionResult<IEnumerable<SubscriptionOrderDto>>> GetUpcomingOrders(int id)
        {
            var orders = await _subscriptionService.GetUpcomingOrdersAsync(id);
            return Ok(orders);
        }

        [HttpPut("{id}/shipping-address")]
        public async Task<IActionResult> UpdateShippingAddress(int id, [FromBody] int addressId)
        {
            var result = await _subscriptionService.UpdateShippingAddressAsync(id, addressId);
            if (!result)
                return NotFound();
            return Ok();
        }

        #endregion

        #region Trial Management

        [HttpPost("start-trial")]
        public async Task<IActionResult> StartTrial([FromQuery] int planId)
        {
            // Security: Use authenticated user's ID instead of parameter
            var userId = GetUserId();
            var result = await _subscriptionService.StartTrialAsync(userId, planId);
            if (!result)
                return BadRequest("Could not start trial");
            return Ok();
        }

        [HttpPost("{id}/end-trial")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EndTrial(int id)
        {
            var result = await _subscriptionService.EndTrialAsync(id);
            if (!result)
                return BadRequest("Could not end trial");
            return Ok();
        }

        [HttpPost("{id}/extend-trial")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExtendTrial(int id, [FromQuery] int days)
        {
            var result = await _subscriptionService.ExtendTrialAsync(id, days);
            if (!result)
                return BadRequest("Could not extend trial");
            return Ok();
        }

        #endregion

        #region Gift Subscriptions

        [HttpPost("gift")]
        public async Task<ActionResult<SubscriptionDto>> CreateGiftSubscription(
            [FromQuery] int recipientUserId,
            [FromQuery] int planId,
            [FromBody] string? message)
        {
            try
            {
                // Security: Use authenticated user's ID as giver instead of parameter
                var giverUserId = GetUserId();
                var subscription = await _subscriptionService.CreateGiftSubscriptionAsync(
                    giverUserId, recipientUserId, planId, message);
                return Ok(subscription);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("gift/redeem")]
        public async Task<IActionResult> RedeemGiftSubscription([FromQuery] string redemptionCode)
        {
            // Security: Use authenticated user's ID instead of parameter
            var userId = GetUserId();
            var result = await _subscriptionService.RedeemGiftSubscriptionAsync(redemptionCode, userId);
            if (!result)
                return BadRequest("Invalid redemption code");
            return Ok();
        }

        #endregion

        #region Returns & RMA

        [HttpPost("returns")]
        public async Task<ActionResult<ReturnRequestDto>> CreateReturn(CreateReturnRequest request)
        {
            try
            {
                var returnRequest = await _subscriptionService.CreateReturnRequestAsync(request);
                return Ok(returnRequest);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("returns/{id}")]
        public async Task<ActionResult<ReturnRequestDto>> GetReturn(int id)
        {
            var returnRequest = await _subscriptionService.GetReturnRequestAsync(id);
            if (returnRequest == null)
                return NotFound();
            return Ok(returnRequest);
        }

        [HttpGet("returns/user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetUserReturns(int userId)
        {
            // Security: Users can only view their own returns unless Admin
            if (!CanAccessUserData(userId))
            {
                return Forbid();
            }
            var returns = await _subscriptionService.GetUserReturnsAsync(userId);
            return Ok(returns);
        }

        [HttpGet("returns")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetAllReturns([FromQuery] ReturnStatus? status = null)
        {
            var returns = await _subscriptionService.GetAllReturnsAsync(status);
            return Ok(returns);
        }

        [HttpPost("returns/{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReturnRequestDto>> ProcessReturn(int id, ProcessReturnRequest request)
        {
            try
            {
                var returnRequest = await _subscriptionService.ProcessReturnAsync(id, request);
                return Ok(returnRequest);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("returns/{id}/tracking")]
        public async Task<IActionResult> UpdateReturnTracking(int id, [FromBody] string trackingNumber)
        {
            var result = await _subscriptionService.UpdateReturnTrackingAsync(id, trackingNumber);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpPost("returns/{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteReturn(int id)
        {
            var result = await _subscriptionService.CompleteReturnAsync(id);
            if (!result)
                return NotFound();
            return Ok();
        }

        #endregion

        #region Abandoned Cart Recovery

        [HttpPost("abandoned-cart/detect")]
        public async Task<ActionResult<AbandonedCartDto>> DetectAbandonedCart(
            [FromQuery] int? userId,
            [FromQuery] string? sessionId)
        {
            try
            {
                var cart = await _subscriptionService.DetectAbandonedCartAsync(userId, sessionId);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("abandoned-carts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AbandonedCartDto>>> GetAbandonedCarts([FromQuery] DateTime? since = null)
        {
            var carts = await _subscriptionService.GetAbandonedCartsAsync(since);
            return Ok(carts);
        }

        [HttpPost("abandoned-cart/send-recovery")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendRecoveryEmail(RecoverAbandonedCartRequest request)
        {
            var result = await _subscriptionService.SendRecoveryEmailAsync(request);
            if (!result)
                return BadRequest("Could not send recovery email");
            return Ok();
        }

        [HttpPost("abandoned-cart/recover")]
        public async Task<IActionResult> RecoverCart([FromQuery] string recoveryCode)
        {
            var result = await _subscriptionService.RecoverCartAsync(recoveryCode);
            if (!result)
                return BadRequest("Invalid recovery code");
            return Ok();
        }

        [HttpPost("abandoned-cart/{id}/mark-recovered")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkCartAsRecovered(int id, [FromQuery] int orderId)
        {
            var result = await _subscriptionService.MarkCartAsRecoveredAsync(id, orderId);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpGet("abandoned-cart/recovery-rate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetRecoveryRate(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var rate = await _subscriptionService.CalculateRecoveryRateAsync(startDate, endDate);
            return Ok(rate);
        }

        #endregion

        #region Gift Cards

        [HttpPost("gift-cards")]
        public async Task<ActionResult<GiftCardDto>> CreateGiftCard(CreateGiftCardRequest request)
        {
            var giftCard = await _subscriptionService.CreateGiftCardAsync(request);
            return Ok(giftCard);
        }

        [HttpGet("gift-cards/{code}")]
        public async Task<ActionResult<GiftCardDto>> GetGiftCard(string code)
        {
            var giftCard = await _subscriptionService.GetGiftCardAsync(code);
            if (giftCard == null)
                return NotFound();
            return Ok(giftCard);
        }

        [HttpGet("gift-cards/{code}/balance")]
        public async Task<ActionResult<decimal>> GetGiftCardBalance(string code)
        {
            var balance = await _subscriptionService.GetGiftCardBalanceAsync(code);
            return Ok(balance);
        }

        [HttpPost("gift-cards/redeem")]
        public async Task<IActionResult> RedeemGiftCard(RedeemGiftCardRequest request)
        {
            var result = await _subscriptionService.RedeemGiftCardAsync(request);
            if (!result)
                return BadRequest("Could not redeem gift card");
            return Ok();
        }

        [HttpPost("gift-cards/{code}/reload")]
        public async Task<IActionResult> ReloadGiftCard(string code, [FromBody] decimal amount)
        {
            var result = await _subscriptionService.ReloadGiftCardAsync(code, amount);
            if (!result)
                return NotFound();
            return Ok();
        }

        [HttpGet("gift-cards/user/{userId}")]
        public async Task<ActionResult<IEnumerable<GiftCardDto>>> GetUserGiftCards(int userId)
        {
            // Security: Users can only view their own gift cards unless Admin
            if (!CanAccessUserData(userId))
            {
                return Forbid();
            }
            var giftCards = await _subscriptionService.GetUserGiftCardsAsync(userId);
            return Ok(giftCards);
        }

        [HttpGet("gift-cards/{code}/transactions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GiftCardTransactionDto>>> GetGiftCardTransactions(string code)
        {
            // Security: Allow Admins or gift card owner (purchaser/recipient) to view transactions
            if (!IsAdmin())
            {
                var giftCard = await _subscriptionService.GetGiftCardAsync(code);
                if (giftCard == null)
                    return NotFound();

                var canAccess = await CanAccessGiftCardAsync(code);
                if (!canAccess)
                    return Forbid();
            }

            var transactions = await _subscriptionService.GetGiftCardTransactionsAsync(code);
            return Ok(transactions);
        }

        [HttpPost("gift-cards/{code}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateGiftCard(string code)
        {
            var result = await _subscriptionService.DeactivateGiftCardAsync(code);
            if (!result)
                return NotFound();
            return Ok();
        }

        #endregion

        #region Analytics

        [HttpGet("analytics/sales")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var analytics = await _subscriptionService.GetSalesAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpGet("analytics/customers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerAnalyticsDto>> GetCustomerAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var analytics = await _subscriptionService.GetCustomerAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpGet("analytics/products")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductAnalyticsDto>> GetProductAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? categoryId = null)
        {
            var analytics = await _subscriptionService.GetProductAnalyticsAsync(startDate, endDate, categoryId);
            return Ok(analytics);
        }

        [HttpGet("analytics/subscriptions")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionAnalyticsDto>> GetSubscriptionAnalytics()
        {
            var analytics = await _subscriptionService.GetSubscriptionAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("analytics/mrr")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetMonthlyRecurringRevenue()
        {
            var mrr = await _subscriptionService.CalculateMonthlyRecurringRevenueAsync();
            return Ok(mrr);
        }

        [HttpGet("analytics/churn-rate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetChurnRate(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var rate = await _subscriptionService.CalculateChurnRateAsync(startDate, endDate);
            return Ok(rate);
        }

        [HttpGet("analytics/customer-lifetime-value")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetCustomerLifetimeValue([FromQuery] int? userId = null)
        {
            var value = await _subscriptionService.CalculateCustomerLifetimeValueAsync(userId);
            return Ok(value);
        }

        #endregion

        #region Batch Operations

        [HttpPost("batch/process-due-subscriptions")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> ProcessDueSubscriptions()
        {
            var processed = await _subscriptionService.ProcessDueSubscriptionsAsync();
            return Ok(new { ProcessedCount = processed });
        }

        [HttpPost("batch/send-payment-reminders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> SendPaymentReminders()
        {
            var sent = await _subscriptionService.SendPaymentRemindersAsync();
            return Ok(new { SentCount = sent });
        }

        [HttpPost("batch/process-abandoned-carts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> ProcessAbandonedCarts()
        {
            var processed = await _subscriptionService.ProcessAbandonedCartsAsync();
            return Ok(new { ProcessedCount = processed });
        }

        [HttpPost("batch/expire-gift-cards")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> ExpireGiftCards()
        {
            var expired = await _subscriptionService.ExpireGiftCardsAsync();
            return Ok(new { ExpiredCount = expired });
        }

        #endregion
    }
}