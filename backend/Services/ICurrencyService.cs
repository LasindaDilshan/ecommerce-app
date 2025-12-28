using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public interface ICurrencyService
{
    Task<List<Currency>> GetAllCurrenciesAsync();
    Task<Currency?> GetCurrencyByCodeAsync(string code);
    Task<decimal> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency);
}
