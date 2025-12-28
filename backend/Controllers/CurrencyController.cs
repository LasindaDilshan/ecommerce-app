using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Currency>>> GetAllCurrencies()
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync();
        return Ok(currencies);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<Currency>> GetCurrencyByCode(string code)
    {
        var currency = await _currencyService.GetCurrencyByCodeAsync(code);

        if (currency == null)
            return NotFound(new { message = "Currency not found" });

        return Ok(currency);
    }

    [HttpGet("convert")]
    public async Task<ActionResult<decimal>> ConvertPrice(
        [FromQuery] decimal amount,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (amount <= 0)
            return BadRequest(new { message = "Amount must be greater than 0" });

        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            return BadRequest(new { message = "From and To currencies are required" });

        var convertedAmount = await _currencyService.ConvertPriceAsync(amount, from, to);
        return Ok(new { amount, from, to, converted = convertedAmount });
    }
}
