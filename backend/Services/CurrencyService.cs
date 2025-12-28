using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(ApplicationDbContext context, ILogger<CurrencyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        return await _context.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Currency?> GetCurrencyByCodeAsync(string code)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && c.IsActive);
    }

    public async Task<decimal> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var from = await GetCurrencyByCodeAsync(fromCurrency);
        var to = await GetCurrencyByCodeAsync(toCurrency);

        if (from == null || to == null)
        {
            _logger.LogWarning($"Currency conversion failed: {fromCurrency} to {toCurrency}");
            return amount;
        }

        // Convert to base currency (USD) first, then to target currency
        var amountInUSD = amount / from.ExchangeRate;
        return amountInUSD * to.ExchangeRate;
    }
}
