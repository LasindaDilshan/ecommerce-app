using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var discountCode = await _newsletterService.SubscribeAsync(request.Email);
        return Ok(new { discountCode, message = "Successfully subscribed! Check your email for your discount code." });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] NewsletterSubscribeRequest request)
    {
        var result = await _newsletterService.UnsubscribeAsync(request.Email);
        if (!result)
        {
            return NotFound(new { message = "Subscription not found" });
        }

        return Ok(new { message = "Successfully unsubscribed" });
    }
}

public record NewsletterSubscribeRequest(string Email);
