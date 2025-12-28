namespace EcommerceAPI.Services;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(int orderId, decimal amount);
    Task<bool> ConfirmPaymentAsync(int orderId, string paymentIntentId);
    Task<bool> ProcessSubscriptionPaymentAsync(string paymentMethodId, decimal amount);
    Task<bool> ProcessRefundAsync(int orderId, decimal amount);
}
