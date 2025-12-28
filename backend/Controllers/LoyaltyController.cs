using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet("account")]
    [Authorize]
    public async Task<ActionResult<LoyaltyAccountDto>> GetAccount()
    {
        var userId = GetUserId();
        var account = await _loyaltyService.GetAccountAsync(userId);
        return Ok(account);
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<ActionResult<LoyaltySummaryDto>> GetSummary()
    {
        var userId = GetUserId();
        var summary = await _loyaltyService.GetSummaryAsync(userId);
        return Ok(summary);
    }

    [HttpGet("transactions")]
    [Authorize]
    public async Task<ActionResult<List<LoyaltyTransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var transactions = await _loyaltyService.GetTransactionsAsync(userId, page, pageSize);
        return Ok(transactions);
    }

    [HttpGet("rewards")]
    [Authorize]
    public async Task<ActionResult<List<LoyaltyRewardDto>>> GetAvailableRewards()
    {
        var userId = GetUserId();
        var rewards = await _loyaltyService.GetAvailableRewardsAsync(userId);
        return Ok(rewards);
    }

    [HttpPost("rewards/{rewardId}/redeem")]
    [Authorize]
    public async Task<ActionResult<RedeemRewardResponse>> RedeemReward(int rewardId)
    {
        var userId = GetUserId();
        var response = await _loyaltyService.RedeemRewardAsync(userId, rewardId);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("redeemed")]
    [Authorize]
    public async Task<ActionResult<List<RedeemedRewardDto>>> GetRedeemedRewards()
    {
        var userId = GetUserId();
        var rewards = await _loyaltyService.GetRedeemedRewardsAsync(userId);
        return Ok(rewards);
    }

    [HttpGet("validate/{code}")]
    public async Task<ActionResult<RedeemedRewardDto>> ValidateCode(string code)
    {
        var reward = await _loyaltyService.ValidateRedemptionCodeAsync(code);

        if (reward == null)
        {
            return NotFound(new { message = "Invalid or expired redemption code" });
        }

        return Ok(reward);
    }

    // Admin endpoints
    [HttpGet("admin/rewards")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<LoyaltyRewardDto>>> GetAllRewards()
    {
        var rewards = await _loyaltyService.GetAllRewardsAsync();
        return Ok(rewards);
    }

    [HttpPost("admin/rewards")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<LoyaltyRewardDto>> CreateReward([FromBody] CreateRewardRequest request)
    {
        var reward = await _loyaltyService.CreateRewardAsync(request);
        return CreatedAtAction(nameof(GetAllRewards), new { id = reward.Id }, reward);
    }

    [HttpPut("admin/rewards/{rewardId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<LoyaltyRewardDto>> UpdateReward(int rewardId, [FromBody] CreateRewardRequest request)
    {
        try
        {
            var reward = await _loyaltyService.UpdateRewardAsync(rewardId, request);
            return Ok(reward);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("admin/rewards/{rewardId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteReward(int rewardId)
    {
        var result = await _loyaltyService.DeleteRewardAsync(rewardId);

        if (!result)
        {
            return NotFound(new { message = "Reward not found" });
        }

        return NoContent();
    }

    [HttpPost("admin/adjust")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AdjustPoints([FromBody] AdjustPointsRequest request)
    {
        var result = await _loyaltyService.AdjustPointsAsync(request.UserId, request.Points, request.Reason);

        if (!result)
        {
            return BadRequest(new { message = "Failed to adjust points" });
        }

        return Ok(new { message = "Points adjusted successfully" });
    }
}
