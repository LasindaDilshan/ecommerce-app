namespace EcommerceAPI.Services;

public interface INewsletterService
{
    Task<string> SubscribeAsync(string email);
    Task<bool> UnsubscribeAsync(string email);
}
