using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EcommerceAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _websiteUrl;

        public EmailService(ISendGridClient sendGridClient, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
            _logger = logger;
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@ecommerce.com";
            _fromName = _configuration["SendGrid:FromName"] ?? "E-Commerce Store";
            _websiteUrl = _configuration["SendGrid:WebsiteUrl"] ?? "https://localhost:4200";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
        {
            try
            {
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent ?? "", htmlContent);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError($"Failed to send email to {toEmail}. Status: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation($"Email sent successfully to {toEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {toEmail}");
                throw;
            }
        }

        public async Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber, decimal totalAmount, string orderDate)
        {
            var subject = $"Order Confirmation - #{orderNumber}";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Order Confirmed!</h1>
                        </div>
                        <div class='content'>
                            <h2>Thank you for your order!</h2>
                            <p>Your order <strong>#{orderNumber}</strong> has been confirmed and is being processed.</p>
                            <p><strong>Order Date:</strong> {orderDate}</p>
                            <p><strong>Total Amount:</strong> ${totalAmount:F2}</p>
                            <p>You will receive a shipping notification once your order has been dispatched.</p>
                            <br>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/orders/{orderNumber}' class='button'>View Order Details</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>If you have any questions, please contact our customer support.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Order Confirmed!

Thank you for your order!
Your order #{orderNumber} has been confirmed and is being processed.

Order Date: {orderDate}
Total Amount: ${totalAmount:F2}

You will receive a shipping notification once your order has been dispatched.

View your order: {_websiteUrl}/orders/{orderNumber}

If you have any questions, please contact our customer support.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendShippingUpdateEmailAsync(string toEmail, string orderNumber, string trackingNumber, string estimatedDelivery)
        {
            var subject = $"Your Order #{orderNumber} Has Been Shipped!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .tracking-box {{ background-color: white; border: 2px solid #2196F3; padding: 15px; margin: 20px 0; text-align: center; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Your Order is On Its Way!</h1>
                        </div>
                        <div class='content'>
                            <h2>Great news!</h2>
                            <p>Your order <strong>#{orderNumber}</strong> has been shipped and is on its way to you.</p>
                            <div class='tracking-box'>
                                <h3>Tracking Number</h3>
                                <p style='font-size: 20px; font-weight: bold;'>{trackingNumber}</p>
                            </div>
                            <p><strong>Estimated Delivery:</strong> {estimatedDelivery}</p>
                            <br>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/track-order/{orderNumber}' class='button'>Track Your Order</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>Thank you for shopping with us!</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Your Order is On Its Way!

Great news!
Your order #{orderNumber} has been shipped and is on its way to you.

Tracking Number: {trackingNumber}
Estimated Delivery: {estimatedDelivery}

Track your order: {_websiteUrl}/track-order/{orderNumber}

Thank you for shopping with us!";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "Password Reset Request";
            var resetUrl = $"{_websiteUrl}/reset-password?token={resetToken}";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #FF9800; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <h2>Reset Your Password</h2>
                            <p>We received a request to reset the password for your account.</p>
                            <p>Click the button below to reset your password:</p>
                            <br>
                            <p style='text-align: center;'>
                                <a href='{resetUrl}' class='button'>Reset Password</a>
                            </p>
                            <br>
                            <div class='warning'>
                                <p><strong>Important:</strong> This link will expire in 1 hour for security reasons.</p>
                            </div>
                            <p>If you didn't request this password reset, please ignore this email. Your password won't be changed.</p>
                        </div>
                        <div class='footer'>
                            <p>For security reasons, this link will expire in 1 hour.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Password Reset Request

We received a request to reset the password for your account.

Click the link below to reset your password:
{resetUrl}

This link will expire in 1 hour for security reasons.

If you didn't request this password reset, please ignore this email. Your password won't be changed.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = $"Welcome to {_fromName}!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .benefits {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to {_fromName}!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {userName}!</h2>
                            <p>Thank you for joining our community! We're excited to have you as part of our family.</p>
                            <div class='benefits'>
                                <h3>As a member, you can:</h3>
                                <ul>
                                    <li>Track your orders in real-time</li>
                                    <li>Save your favorite items to your wishlist</li>
                                    <li>Get exclusive member-only discounts</li>
                                    <li>Enjoy faster checkout with saved addresses</li>
                                    <li>Access your order history anytime</li>
                                </ul>
                            </div>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/products' class='button'>Start Shopping</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>Happy Shopping!</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Welcome to {_fromName}!

Hello {userName}!

Thank you for joining our community! We're excited to have you as part of our family.

As a member, you can:
- Track your orders in real-time
- Save your favorite items to your wishlist
- Get exclusive member-only discounts
- Enjoy faster checkout with saved addresses
- Access your order history anytime

Start shopping: {_websiteUrl}/products

Happy Shopping!";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendOrderStatusUpdateEmailAsync(string toEmail, string orderNumber, string newStatus)
        {
            var subject = $"Order #{orderNumber} Status Update";
            var statusColor = newStatus.ToLower() switch
            {
                "processing" => "#2196F3",
                "shipped" => "#4CAF50",
                "delivered" => "#4CAF50",
                "cancelled" => "#f44336",
                _ => "#9E9E9E"
            };

            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: {statusColor}; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .status-box {{ background-color: white; border: 2px solid {statusColor}; padding: 15px; margin: 20px 0; text-align: center; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: {statusColor}; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Order Status Update</h1>
                        </div>
                        <div class='content'>
                            <h2>Your order status has been updated</h2>
                            <p>Order <strong>#{orderNumber}</strong> status has changed.</p>
                            <div class='status-box'>
                                <h3>New Status</h3>
                                <p style='font-size: 24px; font-weight: bold; color: {statusColor};'>{newStatus}</p>
                            </div>
                            <br>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/orders/{orderNumber}' class='button'>View Order Details</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>Thank you for your patience.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string orderNumber, decimal amount, string paymentMethod)
        {
            var subject = $"Payment Confirmation - Order #{orderNumber}";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .payment-box {{ background-color: white; border: 2px solid #4CAF50; padding: 15px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Payment Received</h1>
                        </div>
                        <div class='content'>
                            <h2>Thank you for your payment!</h2>
                            <p>We have successfully received your payment for order <strong>#{orderNumber}</strong>.</p>
                            <div class='payment-box'>
                                <p><strong>Amount Paid:</strong> ${amount:F2}</p>
                                <p><strong>Payment Method:</strong> {paymentMethod}</p>
                                <p><strong>Transaction Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>
                            </div>
                            <p>Your order is now being processed and you will receive a shipping notification soon.</p>
                        </div>
                        <div class='footer'>
                            <p>Thank you for your business!</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendAccountVerificationEmailAsync(string toEmail, string verificationToken)
        {
            var subject = "Verify Your Email Address";
            var verifyUrl = $"{_websiteUrl}/verify-email?token={verificationToken}";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Verify Your Email</h1>
                        </div>
                        <div class='content'>
                            <h2>Almost there!</h2>
                            <p>Please verify your email address to complete your registration.</p>
                            <p>Click the button below to verify your email:</p>
                            <br>
                            <p style='text-align: center;'>
                                <a href='{verifyUrl}' class='button'>Verify Email Address</a>
                            </p>
                            <br>
                            <p>This link will expire in 24 hours for security reasons.</p>
                        </div>
                        <div class='footer'>
                            <p>If you didn't create an account, please ignore this email.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendOrderCancellationEmailAsync(string toEmail, string orderNumber, string reason)
        {
            var subject = $"Order #{orderNumber} Has Been Cancelled";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Order Cancelled</h1>
                        </div>
                        <div class='content'>
                            <h2>Order Cancellation Confirmation</h2>
                            <p>Your order <strong>#{orderNumber}</strong> has been cancelled.</p>
                            {(string.IsNullOrEmpty(reason) ? "" : $"<p><strong>Reason:</strong> {reason}</p>")}
                            <p>If you paid for this order, a refund will be processed within 3-5 business days.</p>
                            <p>If you have any questions about this cancellation, please contact our customer support.</p>
                        </div>
                        <div class='footer'>
                            <p>We hope to serve you again in the future.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendRefundProcessedEmailAsync(string toEmail, string orderNumber, decimal refundAmount)
        {
            var subject = $"Refund Processed - Order #{orderNumber}";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .refund-box {{ background-color: white; border: 2px solid #4CAF50; padding: 15px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Refund Processed</h1>
                        </div>
                        <div class='content'>
                            <h2>Your refund has been processed</h2>
                            <p>We have processed the refund for order <strong>#{orderNumber}</strong>.</p>
                            <div class='refund-box'>
                                <p><strong>Refund Amount:</strong> ${refundAmount:F2}</p>
                                <p><strong>Processing Date:</strong> {DateTime.Now:yyyy-MM-dd}</p>
                            </div>
                            <p>The refund should appear in your account within 3-5 business days, depending on your payment method.</p>
                        </div>
                        <div class='footer'>
                            <p>Thank you for your understanding.</p>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendSubscriptionWelcomeEmailAsync(string toEmail, string planName, string nextBillingDate)
        {
            var subject = "Welcome to Your Subscription!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .features {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to Your Subscription!</h1>
                        </div>
                        <div class='content'>
                            <h2>Thank you for subscribing!</h2>
                            <p>You've successfully subscribed to: <strong>{planName}</strong></p>
                            <p>Your subscription is now active and will automatically renew on {nextBillingDate}.</p>
                            <div class='features'>
                                <h3>What's Next?</h3>
                                <ul>
                                    <li>Access your subscription benefits immediately</li>
                                    <li>Manage your subscription from your account dashboard</li>
                                    <li>Update payment methods or shipping address anytime</li>
                                </ul>
                            </div>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/account/subscriptions' class='button'>Manage Subscription</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Welcome to Your Subscription!
Thank you for subscribing to: {planName}
Your subscription is now active and will automatically renew on {nextBillingDate}.

What's Next?
- Access your subscription benefits immediately
- Manage your subscription from your account dashboard
- Update payment methods or shipping address anytime";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendSubscriptionPausedEmailAsync(string toEmail, string resumeDate)
        {
            var subject = "Subscription Paused";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .info-box {{ background-color: white; padding: 15px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Subscription Paused</h1>
                        </div>
                        <div class='content'>
                            <p>Your subscription has been successfully paused.</p>
                            <p>It will automatically resume on: <strong>{resumeDate}</strong></p>
                            <div class='info-box'>
                                <p>During the pause period:</p>
                                <ul>
                                    <li>No charges will be made to your account</li>
                                    <li>No deliveries will be scheduled</li>
                                    <li>You can resume your subscription anytime</li>
                                </ul>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Subscription Paused
Your subscription has been successfully paused.
It will automatically resume on: {resumeDate}

During the pause period:
- No charges will be made to your account
- No deliveries will be scheduled
- You can resume your subscription anytime";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendSubscriptionResumedEmailAsync(string toEmail, string nextBillingDate)
        {
            var subject = "Subscription Resumed";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Subscription Resumed</h1>
                        </div>
                        <div class='content'>
                            <p>Great news! Your subscription has been successfully resumed.</p>
                            <p>Next billing date: <strong>{nextBillingDate}</strong></p>
                            <p>Your subscription benefits are now active again.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Subscription Resumed
Great news! Your subscription has been successfully resumed.
Next billing date: {nextBillingDate}
Your subscription benefits are now active again.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendSubscriptionCancelledEmailAsync(string toEmail, string planName, string endDate)
        {
            var subject = "Subscription Cancelled";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #F44336; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Subscription Cancelled</h1>
                        </div>
                        <div class='content'>
                            <p>Your subscription to <strong>{planName}</strong> has been cancelled.</p>
                            <p>Access will continue until: <strong>{endDate}</strong></p>
                            <p>We're sorry to see you go! You can reactivate your subscription anytime before the end date.</p>
                            <p style='text-align: center; margin-top: 30px;'>
                                <a href='{_websiteUrl}/account/subscriptions' class='button'>Reactivate Subscription</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Subscription Cancelled
Your subscription to {planName} has been cancelled.
Access will continue until: {endDate}

We're sorry to see you go! You can reactivate your subscription anytime before the end date.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendSubscriptionReactivatedEmailAsync(string toEmail, string planName)
        {
            var subject = "Subscription Reactivated!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Subscription Reactivated!</h1>
                        </div>
                        <div class='content'>
                            <p>Welcome back! Your subscription to <strong>{planName}</strong> has been successfully reactivated.</p>
                            <p>All your benefits are now restored.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Subscription Reactivated!
Welcome back! Your subscription to {planName} has been successfully reactivated.
All your benefits are now restored.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendPaymentReminderEmailAsync(string toEmail, decimal amount)
        {
            var subject = "Payment Reminder";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Payment Reminder</h1>
                        </div>
                        <div class='content'>
                            <p>This is a reminder that your subscription payment of <strong>${amount:F2}</strong> is past due.</p>
                            <p>Please update your payment method to continue enjoying your subscription benefits.</p>
                            <p style='text-align: center; margin-top: 30px;'>
                                <a href='{_websiteUrl}/account/payment-methods' class='button'>Update Payment Method</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
Payment Reminder
This is a reminder that your subscription payment of ${amount:F2} is past due.
Please update your payment method to continue enjoying your subscription benefits.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendGiftCardEmailAsync(string toEmail, string code, decimal value, string? message)
        {
            var subject = "You've Received a Gift Card!";
            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; background-color: #f9f9f9; }}
                        .gift-box {{ background-color: white; padding: 25px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .code-box {{ background-color: #f0f0f0; padding: 15px; border-radius: 5px; margin: 20px 0; text-align: center; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 25px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎁 You've Received a Gift Card!</h1>
                        </div>
                        <div class='content'>
                            <div class='gift-box'>
                                <h2 style='text-align: center; color: #333;'>Gift Card Value: <span style='color: #4CAF50;'>${value:F2}</span></h2>
                                {(message != null ? $"<p style='font-style: italic; text-align: center; margin: 20px 0;'>\"{message}\"</p>" : "")}
                                <div class='code-box'>
                                    <p style='margin: 5px 0;'>Your Gift Card Code:</p>
                                    <h3 style='color: #667eea; letter-spacing: 2px; margin: 10px 0;'>{code}</h3>
                                </div>
                                <p style='text-align: center;'>Use this code at checkout to apply your gift card balance.</p>
                            </div>
                            <p style='text-align: center; margin-top: 30px;'>
                                <a href='{_websiteUrl}/shop' class='button'>Shop Now</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
You've Received a Gift Card!
Gift Card Value: ${value:F2}
{(message != null ? $"Message: \"{message}\"" : "")}
Your Gift Card Code: {code}
Use this code at checkout to apply your gift card balance.";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task SendAbandonedCartRecoveryEmailAsync(string toEmail, string recoveryCode, decimal cartValue, decimal? discountPercentage)
        {
            var subject = "Complete Your Purchase";
            var discountText = discountPercentage.HasValue ? $"<p style='color: #4CAF50; font-size: 18px; font-weight: bold;'>Complete your order now and save {discountPercentage}%!</p>" : "";

            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .cart-box {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; }}
                        .button {{ display: inline-block; padding: 15px 40px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>You Left Something Behind!</h1>
                        </div>
                        <div class='content'>
                            <p>We noticed you have items worth <strong>${cartValue:F2}</strong> in your cart.</p>
                            {discountText}
                            <div class='cart-box'>
                                <h3>Your items are waiting for you!</h3>
                                <p>Click below to complete your purchase:</p>
                            </div>
                            <p style='text-align: center;'>
                                <a href='{_websiteUrl}/cart?recovery={recoveryCode}' class='button'>Complete My Order</a>
                            </p>
                            <p style='text-align: center; color: #666; margin-top: 20px; font-size: 12px;'>
                                Recovery code: {recoveryCode}
                            </p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 {_fromName}. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var plainTextContent = $@"
You Left Something Behind!
We noticed you have items worth ${cartValue:F2} in your cart.
{(discountPercentage.HasValue ? $"Complete your order now and save {discountPercentage}%!" : "")}

Your items are waiting for you!
Recovery code: {recoveryCode}";

            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }
    }
}