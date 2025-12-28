using Microsoft.EntityFrameworkCore;
using Stripe;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreatePaymentIntentAsync(int orderId, decimal amount)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            throw new Exception("Order not found");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                { "order_id", orderId.ToString() },
                { "order_number", order.OrderNumber }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        order.PaymentIntentId = paymentIntent.Id;
        await _context.SaveChangesAsync();

        return paymentIntent.ClientSecret;
    }

    public async Task<bool> ConfirmPaymentAsync(int orderId, string paymentIntentId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null || order.PaymentIntentId != paymentIntentId)
        {
            return false;
        }

        // Create payment record
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = order.TotalAmount,
            PaymentMethod = "Stripe",
            TransactionId = paymentIntentId,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // Update order status
        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.Processing;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ProcessSubscriptionPaymentAsync(string paymentMethodId, decimal amount)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe uses cents
                Currency = "usd",
                PaymentMethod = paymentMethodId,
                Confirm = true,
                OffSession = true,
                PaymentMethodTypes = new List<string> { "card" }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException)
        {
            return false;
        }
    }

    public async Task<bool> ProcessRefundAsync(int orderId, decimal amount)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || string.IsNullOrEmpty(order.PaymentIntentId))
                return false;

            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = order.PaymentIntentId,
                Amount = (long)(amount * 100) // Stripe uses cents
            };

            var refund = await refundService.CreateAsync(refundOptions);

            // Update payment status
            order.PaymentStatus = PaymentStatus.Refunded;
            await _context.SaveChangesAsync();

            return refund.Status == "succeeded" || refund.Status == "pending";
        }
        catch (StripeException)
        {
            return false;
        }
    }
}
