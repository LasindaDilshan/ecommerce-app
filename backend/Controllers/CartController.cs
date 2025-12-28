using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var cart = await _cartService.AddToCartAsync(GetUserId(), request);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(GetUserId(), cartItemId, request);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var result = await _cartService.RemoveFromCartAsync(GetUserId(), cartItemId);

        if (!result)
        {
            return NotFound(new { message = "Cart item not found" });
        }

        return Ok(new { message = "Item removed from cart" });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(GetUserId());
        return Ok(new { message = "Cart cleared successfully" });
    }
}
