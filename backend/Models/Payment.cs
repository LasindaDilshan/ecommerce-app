namespace EcommerceAPI.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // "Stripe", "PayPal", etc.
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? FailureReason { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}
