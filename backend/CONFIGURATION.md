# Configuration Guide

This document explains how to configure the E-Commerce API for different environments.

## Development Environment

For local development, all configuration is stored in `appsettings.Development.json`. This file contains non-sensitive default values and is safe to commit to source control.

**No additional setup required for development.**

## Production Environment

For production, **NEVER store secrets in appsettings.json files**. Use one of the following methods:

### Option 1: Environment Variables (Recommended)

Set environment variables on your production server using the hierarchical configuration syntax:

```bash
# On Linux/Mac
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=EcommerceDB;User Id=prod_user;Password=SecurePassword123!;TrustServerCertificate=True"
export JwtSettings__SecretKey="YourProductionSecretKeyThatIsAtLeast32CharactersLongAndVerySecure!"
export Stripe__SecretKey="sk_live_your_actual_stripe_key"
export Stripe__PublishableKey="pk_live_your_actual_stripe_key"

# On Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=EcommerceDB;..."
$env:JwtSettings__SecretKey="YourProductionSecretKeyThatIsAtLeast32CharactersLongAndVerySecure!"
```

### Option 2: Azure Key Vault (Enterprise Recommended)

For Azure deployments, use Azure Key Vault:

1. Install the package:
   ```bash
   dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
   ```

2. Update `Program.cs`:
   ```csharp
   builder.Configuration.AddAzureKeyVault(
       new Uri($"https://{keyVaultName}.vault.azure.net/"),
       new DefaultAzureCredential()
   );
   ```

3. Store secrets in Azure Key Vault with the format:
   - `ConnectionStrings--DefaultConnection`
   - `JwtSettings--SecretKey`
   - `Stripe--SecretKey`

### Option 3: User Secrets (Development Only)

For local development with sensitive test data:

```bash
cd Backend
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "YourLocalSecretKey"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_test_key"
```

## Required Configuration

### Critical Secrets (Must Configure for Production)

1. **ConnectionStrings__DefaultConnection**
   - Your production SQL Server connection string
   - Must include proper authentication
   - Example: `Server=prod-server;Database=EcommerceDB;User Id=app_user;Password=SecurePass123!`

2. **JwtSettings__SecretKey**
   - Minimum 32 characters
   - Use a cryptographically secure random string
   - Generate with: `openssl rand -base64 32`

3. **Stripe__SecretKey & Stripe__PublishableKey**
   - Your production Stripe keys (start with `sk_live_` and `pk_live_`)
   - Available from your Stripe Dashboard

4. **CorsOrigins**
   - Your production frontend URLs
   - Example: `["https://yourdomain.com", "https://www.yourdomain.com"]`

### Optional Configuration (Configure in Later Phases)

5. **Email Settings** (Phase 3)
   - SMTP server details or SendGrid API key
   - Required for order confirmations and notifications

6. **Redis Connection** (Phase 6)
   - Required for distributed caching
   - Format: `localhost:6379` or Azure Redis connection string

7. **Application Insights** (Phase 7)
   - Instrumentation key from Azure Portal
   - Required for production monitoring

## Configuration Priority

.NET configuration sources are loaded in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Azure Key Vault (if configured)
6. Command-line arguments

## Security Best Practices

1. **Never commit secrets to source control**
   - `appsettings.json` should have empty values for secrets
   - Use `.gitignore` to exclude `appsettings.Production.json` if created

2. **Use strong secrets**
   - JWT keys: minimum 256 bits (32 characters)
   - Database passwords: strong, unique passwords
   - Change default secrets immediately

3. **Rotate secrets regularly**
   - JWT keys: every 90 days
   - Database passwords: every 180 days
   - API keys: when team members leave

4. **Limit access**
   - Only authorized personnel should access production secrets
   - Use role-based access control in Azure Key Vault
   - Audit secret access regularly

## Verification

To verify your configuration is working:

1. **Check startup logs** - Look for configuration warnings
2. **Test the /swagger endpoint** - Should load without errors
3. **Try authentication** - Login should work with proper JWT generation
4. **Test database** - Application should connect to database successfully

## Troubleshooting

**Problem: "JWT secret key is not configured"**
- Solution: Ensure `JwtSettings__SecretKey` environment variable is set

**Problem: "Database connection failed"**
- Solution: Verify `ConnectionStrings__DefaultConnection` is correct
- Check database server is accessible
- Verify credentials are correct

**Problem: "CORS error in browser"**
- Solution: Add your frontend URL to `CorsOrigins` configuration

**Problem: "Stripe payment failed"**
- Solution: Verify you're using the correct Stripe keys (live vs test)
- Check Stripe dashboard for error details

## Example Production Configuration (Azure App Service)

In Azure App Service Application Settings:

```
ConnectionStrings__DefaultConnection = Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=EcommerceDB;Persist Security Info=False;User ID=youruser;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
JwtSettings__SecretKey = {generate-with-openssl-rand-base64-32}
Stripe__SecretKey = sk_live_xxxxxxxxxxxxxxxxxxxxx
Stripe__PublishableKey = pk_live_xxxxxxxxxxxxxxxxxxxxx
CorsOrigins__0 = https://yourdomain.com
CorsOrigins__1 = https://www.yourdomain.com
```

## Need Help?

- Review .NET Configuration documentation: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
- Review Azure Key Vault integration: https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration
