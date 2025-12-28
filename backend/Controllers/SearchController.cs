using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost("products")]
    public async Task<IActionResult> SearchProducts([FromBody] SearchRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userIdInt = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
            {
                userIdInt = parsedUserId;
            }
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _searchService.SearchProductsAsync(request, userIdInt, ipAddress);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string query, [FromQuery] int limit = 10)
    {
        try
        {
            var request = new AutocompleteRequest
            {
                Query = query,
                Limit = limit
            };

            var result = await _searchService.GetAutocompleteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularSearches([FromQuery] int limit = 10)
    {
        try
        {
            var result = await _searchService.GetPopularSearchesAsync(limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query)
    {
        try
        {
            var result = await _searchService.GetSearchSuggestionsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentSearches([FromQuery] int limit = 10)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId == 0)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var searches = await _searchService.GetRecentSearchesAsync(userId, limit);
            return Ok(new { searches });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
