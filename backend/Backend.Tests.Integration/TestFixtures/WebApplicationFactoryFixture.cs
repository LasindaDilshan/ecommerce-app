using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace Backend.Tests.Integration.TestFixtures
{
    public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // No need to remove - Program.cs handles Testing environment
                // Just ensure we have a unique database name for test isolation
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override configuration for testing
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "InMemory",
                    ["ASPNETCORE_ENVIRONMENT"] = "Testing"
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            // InMemory database cleanup is handled automatically
            // No need to manually delete the database
            base.Dispose(disposing);
        }

        public static void SeedTestData(ApplicationDbContext context)
        {
            // Clear existing data
            context.Users.RemoveRange(context.Users);
            context.Products.RemoveRange(context.Products);
            context.Categories.RemoveRange(context.Categories);
            context.SaveChanges();

            // Seed Categories
            var categories = new[]
            {
                new Category { Id = 1, Name = "Electronics", Description = "Electronic items" },
                new Category { Id = 2, Name = "Clothing", Description = "Clothing items" },
                new Category { Id = 3, Name = "Books", Description = "Books and publications" }
            };
            context.Categories.AddRange(categories);

            // Seed Products
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 1500,
                    StockQuantity = 10,
                    SKU = "LAP001",
                    CategoryId = 1,
                    IsActive = true,
                    IsFeatured = true
                },
                new Product
                {
                    Id = 2,
                    Name = "T-Shirt",
                    Description = "Cotton T-Shirt",
                    Price = 25,
                    StockQuantity = 100,
                    SKU = "TSH001",
                    CategoryId = 2,
                    IsActive = true
                },
                new Product
                {
                    Id = 3,
                    Name = "Programming Book",
                    Description = "Learn programming",
                    Price = 50,
                    StockQuantity = 50,
                    SKU = "BOOK001",
                    CategoryId = 3,
                    IsActive = true
                }
            };
            context.Products.AddRange(products);

            // Seed Users
            var users = new[]
            {
                new User
                {
                    Id = 1,
                    Email = "admin@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    IsActive = true,
                    EmailVerified = true
                },
                new User
                {
                    Id = 2,
                    Email = "user@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                    FirstName = "Test",
                    LastName = "User",
                    Role = "User",
                    IsActive = true,
                    EmailVerified = true
                }
            };
            context.Users.AddRange(users);

            context.SaveChanges();
        }
    }
}