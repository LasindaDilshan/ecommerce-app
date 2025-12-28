using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialProofController : ControllerBase
{
    private readonly ISocialProofService _socialProofService;

    public SocialProofController(ISocialProofService socialProofService)
    {
        _socialProofService = socialProofService;
    }

    [HttpGet("recent-purchases")]
    public async Task<IActionResult> GetRecentPurchases([FromQuery] int limit = 10)
    {
        var purchases = await _socialProofService.GetRecentPurchasesAsync(limit);
        return Ok(purchases);
    }

    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProductSocialProof(int productId)
    {
        var socialProof = await _socialProofService.GetProductSocialProofAsync(productId);
        return Ok(socialProof);
    }
}
