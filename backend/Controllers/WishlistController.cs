using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Services;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identifier");
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<WishlistDto>> GetWishlist()
    {
        var userId = GetUserId();
        var wishlist = await _wishlistService.GetWishlistAsync(userId);
        return Ok(wishlist);
    }

    [HttpPost("add")]
    public async Task<ActionResult<WishlistDto>> AddToWishlist([FromBody] AddToWishlistRequest request)
    {
        try
        {
            var userId = GetUserId();
            var wishlist = await _wishlistService.AddToWishlistAsync(userId, request);
            return Ok(wishlist);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{wishlistItemId}")]
    public async Task<ActionResult> RemoveFromWishlist(int wishlistItemId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, wishlistItemId);

        if (!result)
        {
            return NotFound(new { message = "Wishlist item not found" });
        }

        return Ok(new { message = "Item removed from wishlist" });
    }

    [HttpDelete]
    public async Task<ActionResult> ClearWishlist()
    {
        var userId = GetUserId();
        var result = await _wishlistService.ClearWishlistAsync(userId);

        if (!result)
        {
            return NotFound(new { message = "Wishlist not found" });
        }

        return Ok(new { message = "Wishlist cleared" });
    }

    [HttpGet("check/{productId}")]
    public async Task<ActionResult<bool>> IsInWishlist(int productId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(new { isInWishlist = result });
    }

    [HttpPost("move-to-cart")]
    public async Task<ActionResult<WishlistDto>> MoveToCart([FromBody] MoveToCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            var wishlist = await _wishlistService.MoveToCartAsync(userId, request);
            return Ok(wishlist);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
