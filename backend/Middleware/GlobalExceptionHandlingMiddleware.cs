using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Exceptions;

namespace EcommerceAPI.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Message = exception.Message,
                Details = new Dictionary<string, object>()
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Error = "Not Found";
                    break;

                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "Validation Failed";
                    errorResponse.Details["errors"] = validationEx.Errors;
                    break;

                case AuthenticationException authEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Error = "Authentication Failed";
                    break;

                case AuthorizationException authzEx:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Error = "Authorization Failed";
                    break;

                case ConflictException conflictEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Error = "Conflict";
                    break;

                case BusinessRuleException businessEx:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorResponse.Error = "Business Rule Violation";
                    break;

                case PaymentException paymentEx:
                    response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                    errorResponse.Error = "Payment Failed";
                    break;

                case FileUploadException fileEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "File Upload Failed";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Error = "Internal Server Error";

                    // Don't expose internal error details in production
                    #if DEBUG
                    errorResponse.Message = exception.Message;
                    errorResponse.Details["stackTrace"] = exception.StackTrace ?? "No stack trace available";
                    #else
                    errorResponse.Message = "An unexpected error occurred. Please try again later.";
                    #endif
                    break;
            }

            errorResponse.Details["traceId"] = context.TraceIdentifier;
            errorResponse.Details["timestamp"] = DateTime.UtcNow;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
    }

    // Extension method to register the middleware
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}