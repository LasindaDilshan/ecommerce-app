using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using System.Text;
using System.Xml;
using System.Security;

namespace EcommerceAPI.Controllers;

[ApiController]
public class SitemapController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SitemapController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> GetSitemap()
    {
        var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:4200";

        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Home page
        AddUrl(xml, baseUrl, DateTime.UtcNow, "daily", "1.0");

        // Products page
        AddUrl(xml, $"{baseUrl}/products", DateTime.UtcNow, "daily", "0.9");

        // All active products
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.UpdatedAt })
            .ToListAsync();

        foreach (var product in products)
        {
            AddUrl(xml, $"{baseUrl}/products/{product.Id}", product.UpdatedAt ?? DateTime.UtcNow, "weekly", "0.8");
        }

        // Categories
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.CreatedAt })
            .ToListAsync();

        foreach (var category in categories)
        {
            AddUrl(xml, $"{baseUrl}/products?categoryId={category.Id}", category.CreatedAt, "weekly", "0.7");
        }

        // Static pages
        AddUrl(xml, $"{baseUrl}/cart", DateTime.UtcNow, "always", "0.6");
        AddUrl(xml, $"{baseUrl}/wishlist", DateTime.UtcNow, "always", "0.6");
        AddUrl(xml, $"{baseUrl}/login", DateTime.UtcNow, "monthly", "0.5");
        AddUrl(xml, $"{baseUrl}/register", DateTime.UtcNow, "monthly", "0.5");

        xml.AppendLine("</urlset>");

        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    private void AddUrl(StringBuilder xml, string loc, DateTime lastMod, string changeFreq, string priority)
    {
        xml.AppendLine("  <url>");
        xml.AppendLine($"    <loc>{SecurityElement.Escape(loc)}</loc>");
        xml.AppendLine($"    <lastmod>{lastMod:yyyy-MM-dd}</lastmod>");
        xml.AppendLine($"    <changefreq>{changeFreq}</changefreq>");
        xml.AppendLine($"    <priority>{priority}</priority>");
        xml.AppendLine("  </url>");
    }
}
