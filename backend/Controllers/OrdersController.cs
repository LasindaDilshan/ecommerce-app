using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        IPaymentService paymentService,
        IEmailService emailService,
        IUserService userService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _emailService = emailService;
        _userService = userService;
        _logger = logger;
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

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(GetUserId(), request);

            // Create payment intent
            var clientSecret = await _paymentService.CreatePaymentIntentAsync(order.OrderId, order.TotalAmount);

            return Ok(new { order, clientSecret });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId, GetUserId());

        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(GetUserId());
        return Ok(orders);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(orderId, request);

            // Send status update email
            try
            {
                var user = await _userService.GetUserByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendOrderStatusUpdateEmailAsync(
                        user.Email,
                        order.OrderNumber,
                        order.Status.ToString());

                    // Send shipping notification for shipped status
                    if (order.Status.ToString().ToLower() == "shipped" && !string.IsNullOrEmpty(request.TrackingNumber))
                    {
                        await _emailService.SendShippingUpdateEmailAsync(
                            user.Email,
                            order.OrderNumber,
                            request.TrackingNumber,
                            request.EstimatedDelivery ?? "5-7 business days");
                    }

                    _logger.LogInformation($"Status update email sent for order {order.OrderNumber}");
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, $"Failed to send status update email for order {orderId}");
                // Don't fail the status update if email fails
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{orderId}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(int orderId, [FromBody] string paymentIntentId)
    {
        try
        {
            var result = await _paymentService.ConfirmPaymentAsync(orderId, paymentIntentId);

            if (!result)
            {
                return BadRequest(new { message = "Payment confirmation failed" });
            }

            // Send order confirmation email
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId, GetUserId());
                if (order != null)
                {
                    var user = await _userService.GetUserByIdAsync(order.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendOrderConfirmationEmailAsync(
                            user.Email,
                            order.OrderNumber,
                            order.TotalAmount,
                            order.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

                        await _emailService.SendPaymentConfirmationEmailAsync(
                            user.Email,
                            order.OrderNumber,
                            order.TotalAmount,
                            "Credit Card");

                        _logger.LogInformation($"Order confirmation emails sent for order {order.OrderNumber}");
                    }
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, $"Failed to send order confirmation email for order {orderId}");
                // Don't fail the payment confirmation if email fails
            }

            return Ok(new { message = "Payment confirmed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
