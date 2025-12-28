using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _context;

    public NewsletterService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> SubscribeAsync(string email)
    {
        // Check if already subscribed
        var existing = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(n => n.Email == email);

        if (existing != null)
        {
            if (existing.IsActive)
            {
                return existing.DiscountCode ?? "ALREADY_SUBSCRIBED";
            }

            // Reactivate subscription
            existing.IsActive = true;
            existing.SubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing.DiscountCode ?? "NEWSLETTER5";
        }

        // Create new subscription with discount code
        var discountCode = $"NEWSLETTER5-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

        var subscription = new NewsletterSubscription
        {
            Email = email,
            DiscountCode = discountCode,
            IsActive = true
        };

        _context.NewsletterSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return discountCode;
    }

    public async Task<bool> UnsubscribeAsync(string email)
    {
        var subscription = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(n => n.Email == email && n.IsActive);

        if (subscription == null)
            return false;

        subscription.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}
