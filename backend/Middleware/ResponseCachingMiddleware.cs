using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Middleware;

public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseCachingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add cache headers for static assets
        var path = context.Request.Path.Value?.ToLower();

        if (path != null && (path.Contains("/uploads/") || path.EndsWith(".jpg") ||
            path.EndsWith(".png") || path.EndsWith(".gif") || path.EndsWith(".svg") ||
            path.EndsWith(".css") || path.EndsWith(".js")))
        {
            context.Response.Headers["Cache-Control"] = "public, max-age=31536000"; // 1 year
            context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
        }
        else if (path != null && path.StartsWith("/api/"))
        {
            // API responses - short cache or no cache depending on endpoint
            if (path.Contains("/products") && !path.Contains("/admin"))
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=300"; // 5 minutes
            }
            else
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            }
        }

        await _next(context);
    }
}

public static class ResponseCachingMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseCachingHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCachingMiddleware>();
    }
}
