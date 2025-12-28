using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Stripe;
using System.IO;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret not configured");
            return BadRequest("Webhook secret not configured");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            // Handle the event
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentSucceeded(paymentIntent!);
                    break;

                case Events.PaymentIntentPaymentFailed:
                    paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentFailed(paymentIntent!);
                    break;

                case Events.PaymentIntentCanceled:
                    paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentCanceled(paymentIntent!);
                    break;

                case Events.ChargeRefunded:
                    var charge = stripeEvent.Data.Object as Charge;
                    await HandleChargeRefunded(charge!);
                    break;

                case Events.PaymentIntentRequiresAction:
                    paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("Payment intent requires action: {PaymentIntentId}", paymentIntent?.Id);
                    break;

                default:
                    _logger.LogWarning("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe webhook signature verification failed");
            return StatusCode(400, new { error = "Webhook signature verification failed" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing webhook");
            return StatusCode(500);
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
    {
        _logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent.Id);

        // Extract order ID from metadata
        if (paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString)
            && int.TryParse(orderIdString, out var orderId))
        {
            var confirmed = await _paymentService.ConfirmPaymentAsync(orderId, paymentIntent.Id);

            if (confirmed)
            {
                _logger.LogInformation("Order {OrderId} payment confirmed", orderId);

                // Send confirmation email
                try
                {
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order != null)
                    {
                        await _emailService.SendOrderConfirmationEmailAsync(
                            order.CustomerEmail,
                            order.OrderNumber,
                            order.TotalAmount,
                            order.OrderDate.ToString("yyyy-MM-dd")
                        );
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to send confirmation email for order {OrderId}", orderId);
                }
            }
            else
            {
                _logger.LogWarning("Failed to confirm payment for order {OrderId}", orderId);
            }
        }
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
    {
        _logger.LogWarning("Payment failed: {PaymentIntentId}, Reason: {Reason}",
            paymentIntent.Id,
            paymentIntent.LastPaymentError?.Message);

        if (paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString)
            && int.TryParse(orderIdString, out var orderId))
        {
            var request = new UpdateOrderStatusRequest
            {
                Status = OrderStatus.Cancelled
            };
            await _orderService.UpdateOrderStatusAsync(orderId, request);
            _logger.LogInformation("Order {OrderId} marked as payment failed", orderId);
        }
    }

    private async Task HandlePaymentIntentCanceled(PaymentIntent paymentIntent)
    {
        _logger.LogInformation("Payment canceled: {PaymentIntentId}", paymentIntent.Id);

        if (paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString)
            && int.TryParse(orderIdString, out var orderId))
        {
            var request = new UpdateOrderStatusRequest
            {
                Status = OrderStatus.Cancelled
            };
            await _orderService.UpdateOrderStatusAsync(orderId, request);
            _logger.LogInformation("Order {OrderId} marked as cancelled", orderId);
        }
    }

    private async Task HandleChargeRefunded(Charge charge)
    {
        _logger.LogInformation("Charge refunded: {ChargeId}, Amount: {Amount}",
            charge.Id,
            charge.AmountRefunded);

        // The refund is already processed in PaymentService.ProcessRefundAsync
        // This webhook confirms the refund was successful
        await Task.CompletedTask;
    }

    [HttpPost("refund/{orderId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ProcessRefund(int orderId, [FromBody] decimal amount)
    {
        try
        {
            var success = await _paymentService.ProcessRefundAsync(orderId, amount);

            if (success)
            {
                return Ok(new { message = "Refund processed successfully" });
            }

            return BadRequest(new { message = "Failed to process refund" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing refund for order {OrderId}", orderId);
            return StatusCode(500, new { message = "An error occurred while processing the refund" });
        }
    }
}
