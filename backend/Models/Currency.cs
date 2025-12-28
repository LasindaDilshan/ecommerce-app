namespace EcommerceAPI.Models;

public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // USD, EUR, GBP, etc.
    public string Symbol { get; set; } = string.Empty; // $, €, £, etc.
    public string Name { get; set; } = string.Empty; // US Dollar, Euro, British Pound, etc.
    public decimal ExchangeRate { get; set; } // Rate relative to base currency (USD)
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
