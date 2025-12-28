using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using Backend.DTOs;
using System.Security.Claims;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/guest/cart")]
public class GuestCartController : ControllerBase
{
    private readonly ICartService _cartService;

    public GuestCartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get guest cart by session ID
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetGuestCart([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var cart = await _cartService.GetGuestCartAsync(sessionId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add item to guest cart
    /// </summary>
    [HttpPost("add")]
    public async Task<ActionResult<CartDto>> AddToGuestCart([FromBody] AddToGuestCartRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var addToCartRequest = new AddToCartRequest
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var cart = await _cartService.AddToGuestCartAsync(request.SessionId, addToCartRequest);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update guest cart item quantity
    /// </summary>
    [HttpPut("{cartItemId}")]
    public async Task<ActionResult<CartDto>> UpdateGuestCartItem(
        int cartItemId,
        [FromQuery] string sessionId,
        [FromBody] UpdateGuestCartItemRequest request)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var updateRequest = new UpdateCartItemRequest
            {
                Quantity = request.Quantity
            };

            var cart = await _cartService.UpdateGuestCartItemAsync(sessionId, cartItemId, updateRequest);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove item from guest cart
    /// </summary>
    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromGuestCart(
        int cartItemId,
        [FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var result = await _cartService.RemoveFromGuestCartAsync(sessionId, cartItemId);
            if (result)
            {
                return Ok(new { message = "Item removed from cart" });
            }
            return NotFound(new { message = "Cart item not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clear guest cart
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearGuestCart([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var result = await _cartService.ClearGuestCartAsync(sessionId);
            if (result)
            {
                return Ok(new { message = "Cart cleared" });
            }
            return NotFound(new { message = "Cart not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Apply coupon to guest cart
    /// </summary>
    [HttpPost("apply-coupon")]
    public async Task<ActionResult<CartDto>> ApplyCouponToGuestCart([FromBody] ApplyCouponRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest("Session ID is required");
        }

        if (string.IsNullOrEmpty(request.CouponCode))
        {
            return BadRequest("Coupon code is required");
        }

        try
        {
            var cart = await _cartService.ApplyCouponToGuestCartAsync(request.SessionId, request.CouponCode);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove coupon from guest cart
    /// </summary>
    [HttpDelete("remove-coupon")]
    public async Task<ActionResult<CartDto>> RemoveCouponFromGuestCart([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var cart = await _cartService.RemoveCouponFromGuestCartAsync(sessionId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Merge guest cart with user cart (requires authentication)
    /// </summary>
    [HttpPost("merge")]
    [Authorize]
    public async Task<ActionResult<CartDto>> MergeGuestCart([FromBody] MergeGuestCartRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest("Session ID is required");
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var cart = await _cartService.MergeGuestCartWithUserCartAsync(request.SessionId, userId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
