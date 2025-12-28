using EcommerceAPI.Models;

namespace EcommerceAPI.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            return;
        }

        // Seed Admin User
        var adminUser = new User
        {
            Email = "admin@ecommerce.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            PhoneNumber = "1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Seed Regular User
        var regularUser = new User
        {
            Email = "user@ecommerce.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FirstName = "John",
            LastName = "Doe",
            Role = "User",
            PhoneNumber = "0987654321",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, regularUser);
        await context.SaveChangesAsync();

        // Seed Categories
        var electronics = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            IsActive = true
        };

        var clothing = new Category
        {
            Name = "Clothing",
            Description = "Fashion and apparel",
            IsActive = true
        };

        var books = new Category
        {
            Name = "Books",
            Description = "Books and educational materials",
            IsActive = true
        };

        var home = new Category
        {
            Name = "Home & Garden",
            Description = "Home improvement and garden supplies",
            IsActive = true
        };

        context.Categories.AddRange(electronics, clothing, books, home);
        await context.SaveChangesAsync();

        // Seed Products
        var laptopProduct = new Product
        {
            Name = "Laptop Pro 15",
            Description = "High-performance laptop with 16GB RAM and 512GB SSD",
            Price = 1299.99m,
            DiscountPrice = 1199.99m,
            StockQuantity = 50,
            SKU = "ELEC-LAP-001",
            CategoryId = electronics.Id,
            IsActive = true,
            IsFeatured = true,
            ImageUrl = "https://placehold.co/600x400/4A90E2/FFFFFF?text=Laptop+Pro+15"
        };

        var mouseProduct = new Product
        {
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse with 6 buttons",
            Price = 29.99m,
            StockQuantity = 200,
            SKU = "ELEC-MOU-001",
            CategoryId = electronics.Id,
            IsActive = true,
            IsFeatured = false,
            ImageUrl = "https://placehold.co/600x400/7B68EE/FFFFFF?text=Wireless+Mouse"
        };

        var tshirtProduct = new Product
        {
            Name = "Men's T-Shirt",
            Description = "100% cotton comfortable t-shirt",
            Price = 19.99m,
            DiscountPrice = 14.99m,
            StockQuantity = 150,
            SKU = "CLO-TSH-001",
            CategoryId = clothing.Id,
            IsActive = true,
            IsFeatured = true,
            ImageUrl = "https://placehold.co/600x400/50C878/FFFFFF?text=Men's+T-Shirt"
        };

        var bookProduct = new Product
        {
            Name = "Programming Guide Book",
            Description = "Complete guide to modern programming",
            Price = 49.99m,
            StockQuantity = 75,
            SKU = "BOO-PRG-001",
            CategoryId = books.Id,
            IsActive = true,
            IsFeatured = false,
            ImageUrl = "https://placehold.co/600x400/FF6347/FFFFFF?text=Programming+Book"
        };

        var lampProduct = new Product
        {
            Name = "LED Desk Lamp",
            Description = "Adjustable LED desk lamp with USB charging",
            Price = 39.99m,
            StockQuantity = 100,
            SKU = "HOM-LAM-001",
            CategoryId = home.Id,
            IsActive = true,
            IsFeatured = true,
            ImageUrl = "https://placehold.co/600x400/FFD700/000000?text=LED+Desk+Lamp"
        };

        context.Products.AddRange(laptopProduct, mouseProduct, tshirtProduct, bookProduct, lampProduct);
        await context.SaveChangesAsync();

        // Seed Product Images
        var productImages = new List<ProductImage>
        {
            // Laptop images
            new ProductImage
            {
                ProductId = laptopProduct.Id,
                ImageUrl = "https://placehold.co/600x400/4A90E2/FFFFFF?text=Laptop+Front",
                IsPrimary = true,
                DisplayOrder = 1
            },
            new ProductImage
            {
                ProductId = laptopProduct.Id,
                ImageUrl = "https://placehold.co/600x400/3A7BC8/FFFFFF?text=Laptop+Side",
                IsPrimary = false,
                DisplayOrder = 2
            },
            new ProductImage
            {
                ProductId = laptopProduct.Id,
                ImageUrl = "https://placehold.co/600x400/2A6BB8/FFFFFF?text=Laptop+Keyboard",
                IsPrimary = false,
                DisplayOrder = 3
            },
            // Mouse images
            new ProductImage
            {
                ProductId = mouseProduct.Id,
                ImageUrl = "https://placehold.co/600x400/7B68EE/FFFFFF?text=Mouse+Front",
                IsPrimary = true,
                DisplayOrder = 1
            },
            new ProductImage
            {
                ProductId = mouseProduct.Id,
                ImageUrl = "https://placehold.co/600x400/6B58DE/FFFFFF?text=Mouse+Side",
                IsPrimary = false,
                DisplayOrder = 2
            },
            // T-Shirt images
            new ProductImage
            {
                ProductId = tshirtProduct.Id,
                ImageUrl = "https://placehold.co/600x400/50C878/FFFFFF?text=T-Shirt+Front",
                IsPrimary = true,
                DisplayOrder = 1
            },
            new ProductImage
            {
                ProductId = tshirtProduct.Id,
                ImageUrl = "https://placehold.co/600x400/40B868/FFFFFF?text=T-Shirt+Back",
                IsPrimary = false,
                DisplayOrder = 2
            },
            new ProductImage
            {
                ProductId = tshirtProduct.Id,
                ImageUrl = "https://placehold.co/600x400/30A858/FFFFFF?text=T-Shirt+Detail",
                IsPrimary = false,
                DisplayOrder = 3
            },
            // Book images
            new ProductImage
            {
                ProductId = bookProduct.Id,
                ImageUrl = "https://placehold.co/600x400/FF6347/FFFFFF?text=Book+Cover",
                IsPrimary = true,
                DisplayOrder = 1
            },
            new ProductImage
            {
                ProductId = bookProduct.Id,
                ImageUrl = "https://placehold.co/600x400/EF5337/FFFFFF?text=Book+Back",
                IsPrimary = false,
                DisplayOrder = 2
            },
            // Lamp images
            new ProductImage
            {
                ProductId = lampProduct.Id,
                ImageUrl = "https://placehold.co/600x400/FFD700/000000?text=Lamp+On",
                IsPrimary = true,
                DisplayOrder = 1
            },
            new ProductImage
            {
                ProductId = lampProduct.Id,
                ImageUrl = "https://placehold.co/600x400/EFC700/000000?text=Lamp+Off",
                IsPrimary = false,
                DisplayOrder = 2
            },
            new ProductImage
            {
                ProductId = lampProduct.Id,
                ImageUrl = "https://placehold.co/600x400/DFB700/000000?text=Lamp+Angle",
                IsPrimary = false,
                DisplayOrder = 3
            }
        };

        context.ProductImages.AddRange(productImages);
        await context.SaveChangesAsync();

        // Create cart for regular user
        var cart = new Cart
        {
            UserId = regularUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();
    }
}
