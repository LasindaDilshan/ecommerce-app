using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/discount-codes")]
    public class DiscountCodesController : ControllerBase
    {
        private readonly IDiscountCodeService _discountCodeService;

        public DiscountCodesController(IDiscountCodeService discountCodeService)
        {
            _discountCodeService = discountCodeService;
        }

        // Public endpoint - Validate a coupon code
        [HttpPost("validate")]
        public async Task<ActionResult<CouponValidationResponse>> ValidateCoupon([FromBody] ApplyCouponRequest request)
        {
            try
            {
                int? userId = null;
                string? guestEmail = null;

                // Check if user is authenticated
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && int.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
                else
                {
                    guestEmail = request.SessionId; // Use sessionId as identifier for guests
                }

                // For validation, we need the cart subtotal and product IDs
                // This would typically come from the cart service, but for validation purposes
                // we'll accept them in the request or get from current cart

                var result = await _discountCodeService.ValidateCouponAsync(
                    request.CouponCode,
                    userId,
                    guestEmail,
                    0, // Subtotal will be calculated by service from cart
                    new List<int>() // ProductIds will be fetched from cart by service
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Admin Endpoints - Protected by authorization

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<DiscountCodeDto>>> GetAllDiscountCodes([FromQuery] bool activeOnly = false)
        {
            try
            {
                var coupons = await _discountCodeService.GetAllDiscountCodesAsync(activeOnly);
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DiscountCodeDto>> GetDiscountCodeById(int id)
        {
            try
            {
                var coupon = await _discountCodeService.GetDiscountCodeByIdAsync(id);
                if (coupon == null)
                {
                    return NotFound(new { message = "Discount code not found" });
                }
                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-code/{code}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DiscountCodeDto>> GetDiscountCodeByCode(string code)
        {
            try
            {
                var coupon = await _discountCodeService.GetDiscountCodeByCodeAsync(code);
                if (coupon == null)
                {
                    return NotFound(new { message = "Discount code not found" });
                }
                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DiscountCodeDto>> CreateDiscountCode([FromBody] CreateDiscountCodeRequest request)
        {
            try
            {
                var coupon = await _discountCodeService.CreateDiscountCodeAsync(request);
                return CreatedAtAction(nameof(GetDiscountCodeById), new { id = coupon.Id }, coupon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DiscountCodeDto>> UpdateDiscountCode(int id, [FromBody] UpdateDiscountCodeRequest request)
        {
            try
            {
                var coupon = await _discountCodeService.UpdateDiscountCodeAsync(id, request);
                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDiscountCode(int id)
        {
            try
            {
                var result = await _discountCodeService.DeleteDiscountCodeAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Discount code not found" });
                }
                return Ok(new { message = "Discount code deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DiscountCodeStatsDto>> GetStats()
        {
            try
            {
                var stats = await _discountCodeService.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
