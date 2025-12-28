using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;
using Backend.DTOs;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/guest/checkout")]
public class GuestCheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;

    public GuestCheckoutController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Create guest order from guest cart
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GuestOrderResponse>> CreateGuestOrder([FromBody] GuestCheckoutRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest(new { message = "Session ID is required" });
        }

        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
        {
            return BadRequest(new { message = "First name and last name are required" });
        }

        if (request.ShippingAddress == null)
        {
            return BadRequest(new { message = "Shipping address is required" });
        }

        try
        {
            var order = await _orderService.CreateGuestOrderAsync(request);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Track guest order by order number and email
    /// </summary>
    [HttpGet("track")]
    public async Task<ActionResult<GuestOrderResponse>> TrackGuestOrder(
        [FromQuery] string orderNumber,
        [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(orderNumber))
        {
            return BadRequest(new { message = "Order number is required" });
        }

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        try
        {
            var order = await _orderService.GetGuestOrderByNumberAsync(orderNumber, email);
            if (order == null)
            {
                return NotFound(new { message = "Order not found or email doesn't match" });
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
