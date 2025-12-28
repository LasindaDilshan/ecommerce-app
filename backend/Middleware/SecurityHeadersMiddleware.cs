namespace EcommerceAPI.Middleware;

/// <summary>
/// Middleware to add security headers to HTTP responses
/// Protects against XSS, clickjacking, content sniffing, and other attacks
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent XSS attacks by enabling browser's XSS filter
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Prevent clickjacking by disallowing the page to be embedded in iframes
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy - adjust based on your needs
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://js.stripe.com; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https: blob:; " +
            "font-src 'self'; " +
            "connect-src 'self' https://api.stripe.com; " +
            "frame-src https://js.stripe.com https://hooks.stripe.com; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';");

        // Permissions Policy - restrict browser features
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(self), usb=()");

        // Cache control for sensitive responses
        if (context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/users"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
