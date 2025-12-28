using System.Threading.Tasks;

namespace EcommerceAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
        Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber, decimal totalAmount, string orderDate);
        Task SendShippingUpdateEmailAsync(string toEmail, string orderNumber, string trackingNumber, string estimatedDelivery);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendOrderStatusUpdateEmailAsync(string toEmail, string orderNumber, string newStatus);
        Task SendPaymentConfirmationEmailAsync(string toEmail, string orderNumber, decimal amount, string paymentMethod);
        Task SendAccountVerificationEmailAsync(string toEmail, string verificationToken);
        Task SendOrderCancellationEmailAsync(string toEmail, string orderNumber, string reason);
        Task SendRefundProcessedEmailAsync(string toEmail, string orderNumber, decimal refundAmount);

        // Subscription emails
        Task SendSubscriptionWelcomeEmailAsync(string toEmail, string planName, string nextBillingDate);
        Task SendSubscriptionPausedEmailAsync(string toEmail, string resumeDate);
        Task SendSubscriptionResumedEmailAsync(string toEmail, string nextBillingDate);
        Task SendSubscriptionCancelledEmailAsync(string toEmail, string planName, string endDate);
        Task SendSubscriptionReactivatedEmailAsync(string toEmail, string planName);
        Task SendPaymentReminderEmailAsync(string toEmail, decimal amount);

        // Gift card emails
        Task SendGiftCardEmailAsync(string toEmail, string code, decimal value, string? message);
        Task SendAbandonedCartRecoveryEmailAsync(string toEmail, string recoveryCode, decimal cartValue, decimal? discountPercentage);
    }
}