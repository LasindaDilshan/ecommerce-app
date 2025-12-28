using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text;
using EcommerceAPI.Data;
using EcommerceAPI.Services;
using Backend.Services;
using Serilog;
using AspNetCoreRateLimit;
using EcommerceAPI.Middleware;
using FluentValidation;
using SendGrid;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/css",
        "text/html",
        "application/javascript",
        "text/javascript"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
// Only configure PostgreSQL if not in Testing environment (integration tests will override)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    // Placeholder for testing - will be overridden by test fixture
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestingDb"));
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>()!)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Rate Limiting
builder.Services.AddRateLimiting(builder.Configuration);

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRecommendationService, ProductRecommendationService>();
builder.Services.AddScoped<ISocialProofService, SocialProofService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IProductQAService, ProductQAService>();

// Configure SendGrid Email Service
var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];
if (!string.IsNullOrEmpty(sendGridApiKey))
{
    builder.Services.AddScoped<ISendGridClient>(provider => new SendGridClient(sendGridApiKey));
    builder.Services.AddScoped<IEmailService, EmailService>();
}
else
{
    // Register a dummy email service if SendGrid is not configured
    builder.Services.AddScoped<ISendGridClient>(provider => new SendGridClient("dummy-key"));
    builder.Services.AddScoped<IEmailService, EmailService>();
    Log.Warning("SendGrid API key not configured. Email functionality will not work.");
}

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Global exception handling
app.UseGlobalExceptionHandling();

// Add security headers
app.UseSecurityHeaders();

// Enable HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Enable response compression (must be early in pipeline)
app.UseResponseCompression();

// Add response caching headers
app.UseResponseCachingHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Rate limiting must be before authentication
app.UseIpRateLimiting();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<EcommerceAPI.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<EcommerceAPI.Hubs.ChatHub>("/hubs/chat");
app.MapHub<EcommerceAPI.Hubs.ActivityHub>("/hubs/activity");

// Ensure database is created and seeded (skip for testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context);
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
