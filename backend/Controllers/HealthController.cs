using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public HealthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var status = "Healthy";
        var services = new Dictionary<string, string>();

        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();
            services.Add("Database", "Healthy");
        }
        catch
        {
            services.Add("Database", "Unhealthy");
            status = "Degraded";
        }

        // Check if migrations are up to date
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            services.Add("Migrations", pendingMigrations.Any() ? "Pending" : "Up to date");
        }
        catch
        {
            services.Add("Migrations", "Unknown");
        }

        return Ok(new
        {
            Status = status,
            Timestamp = DateTime.UtcNow,
            Services = services
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            // Check if database is accessible and migrations are applied
            await _context.Database.CanConnectAsync();
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                return StatusCode(503, new { Status = "Not Ready", Reason = "Database migrations pending" });
            }

            return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { Status = "Not Ready", Reason = ex.Message });
        }
    }

    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        // Simple liveness check - if the application can respond, it's alive
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }
}