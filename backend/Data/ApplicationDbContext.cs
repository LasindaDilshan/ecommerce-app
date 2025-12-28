using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Models;
using Backend.Models;

namespace EcommerceAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    public DbSet<DiscountCodeUsage> DiscountCodeUsages { get; set; }
    public DbSet<DiscountCodeProduct> DiscountCodeProducts { get; set; }
    public DbSet<DiscountCodeCategory> DiscountCodeCategories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewVote> ReviewVotes { get; set; }
    public DbSet<ProductRating> ProductRatings { get; set; }

    // Inventory Management
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<StockBatch> StockBatches { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierProduct> SupplierProducts { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    // Subscription System
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionPlanProduct> SubscriptionPlanProducts { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<SubscriptionOrder> SubscriptionOrders { get; set; }
    public DbSet<SubscriptionModification> SubscriptionModifications { get; set; }
    public DbSet<GiftSubscription> GiftSubscriptions { get; set; }

    // Returns & RMA
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }

    // Abandoned Cart Recovery
    public DbSet<AbandonedCart> AbandonedCarts { get; set; }

    // Gift Cards
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<GiftCardTransaction> GiftCardTransactions { get; set; }

    // Newsletter
    public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }

    // Currency
    public DbSet<Currency> Currencies { get; set; }

    // Real-time Features
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // Advanced Search
    public DbSet<SearchLog> SearchLogs { get; set; }

    // Loyalty Program
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public DbSet<LoyaltyReward> LoyaltyRewards { get; set; }
    public DbSet<RedeemedReward> RedeemedRewards { get; set; }

    // Product Q&A
    public DbSet<ProductQuestion> ProductQuestions { get; set; }
    public DbSet<ProductAnswer> ProductAnswers { get; set; }
    public DbSet<QuestionVote> QuestionVotes { get; set; }
    public DbSet<AnswerVote> AnswerVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RefreshToken)
                .WithOne(r => r.User)
                .HasForeignKey<RefreshToken>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Wishlist)
                .WithOne(w => w.User)
                .HasForeignKey<Wishlist>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SKU).IsUnique();
            // Add index for category filtering
            entity.HasIndex(e => e.CategoryId);
            // Add index for active products
            entity.HasIndex(e => e.IsActive);
            // Add index for featured products
            entity.HasIndex(e => e.IsFeatured);
            // Add composite index for common product queries
            entity.HasIndex(e => new { e.IsActive, e.CategoryId });
            // Add index for price range queries
            entity.HasIndex(e => e.Price);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure optimistic locking with RowVersion
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Cart Configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId);
            // Add index for user cart lookup
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.SessionId).HasMaxLength(255);
            entity.Property(e => e.AppliedCouponCode).HasMaxLength(50);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
        });

        // CartItem Configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            // Add index for user orders lookup (frequently queried)
            entity.HasIndex(e => e.UserId);
            // Add index for order status filtering
            entity.HasIndex(e => e.Status);
            // Add composite index for user orders sorted by date
            entity.HasIndex(e => new { e.UserId, e.OrderDate });
            // Add index for guest order lookup
            entity.HasIndex(e => new { e.GuestEmail, e.OrderNumber });

            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Guest order fields
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.GuestFirstName).HasMaxLength(100);
            entity.Property(e => e.GuestLastName).HasMaxLength(100);

            entity.HasOne(e => e.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        });

        // Address Configuration
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        // ProductImage Configuration
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Wishlist Configuration
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // WishlistItem Configuration
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(e => e.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DiscountCode Configuration
        modelBuilder.Entity<DiscountCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumPurchase).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaximumDiscount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // DiscountCodeUsage Configuration
        modelBuilder.Entity<DiscountCodeUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.DiscountApplied).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.DiscountCode)
                .WithMany(d => d.Usages)
                .HasForeignKey(e => e.DiscountCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DiscountCodeProduct Configuration (Many-to-Many)
        modelBuilder.Entity<DiscountCodeProduct>(entity =>
        {
            entity.HasKey(e => new { e.DiscountCodeId, e.ProductId });

            entity.HasOne(e => e.DiscountCode)
                .WithMany(d => d.ApplicableProducts)
                .HasForeignKey(e => e.DiscountCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiscountCodeCategory Configuration (Many-to-Many)
        modelBuilder.Entity<DiscountCodeCategory>(entity =>
        {
            entity.HasKey(e => new { e.DiscountCodeId, e.CategoryId });

            entity.HasOne(e => e.DiscountCode)
                .WithMany(d => d.ApplicableCategories)
                .HasForeignKey(e => e.DiscountCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review Configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Rating).IsRequired();

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Composite index for faster queries
            entity.HasIndex(e => new { e.ProductId, e.IsApproved });
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
        });

        // ReviewVote Configuration
        modelBuilder.Entity<ReviewVote>(entity =>
        {
            entity.HasKey(e => e.VoteId);

            entity.HasOne(e => e.Review)
                .WithMany(r => r.ReviewVotes)
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure one vote per user per review
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
        });

        // ProductRating Configuration
        modelBuilder.Entity<ProductRating>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.HasOne(e => e.Product)
                .WithOne()
                .HasForeignKey<ProductRating>(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3,2)");
        });

        // Warehouse Configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
        });

        // StockItem Configuration
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasKey(e => e.StockItemId);
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.StockItems)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockBatch Configuration
        modelBuilder.Entity<StockBatch>(entity =>
        {
            entity.HasKey(e => e.BatchId);
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PurchaseCost).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.StockItem)
                .WithMany(s => s.Batches)
                .HasForeignKey(e => e.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StockReservation Configuration
        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId);

            entity.HasOne(e => e.StockItem)
                .WithMany(s => s.Reservations)
                .HasForeignKey(e => e.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StockTransfer Configuration
        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId);
            entity.Property(e => e.TransferNumber).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.FromWarehouse)
                .WithMany(w => w.OutgoingTransfers)
                .HasForeignKey(e => e.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToWarehouse)
                .WithMany(w => w.IncomingTransfers)
                .HasForeignKey(e => e.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StockTransferItem Configuration
        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasKey(e => e.TransferItemId);

            entity.HasOne(e => e.Transfer)
                .WithMany(t => t.TransferItems)
                .HasForeignKey(e => e.TransferId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockMovement Configuration
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId);

            entity.HasOne(e => e.StockItem)
                .WithMany()
                .HasForeignKey(e => e.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
        });

        // SupplierProduct Configuration
        modelBuilder.Entity<SupplierProduct>(entity =>
        {
            entity.HasKey(e => e.SupplierProductId);
            entity.HasIndex(e => new { e.SupplierId, e.ProductId }).IsUnique();
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.SupplierProducts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseOrder Configuration
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseOrderItem Configuration
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderItemId);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Currency Configuration
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(5);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)");
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Link).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
        });

        // ChatSession Configuration
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChatRoomId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.FeedbackComment).HasMaxLength(1000);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SupportAgent)
                .WithMany()
                .HasForeignKey(e => e.SupportAgentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ChatRoomId).IsUnique();
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });

        // ChatMessage Configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.MessageType).HasMaxLength(50);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);

            entity.HasOne(e => e.ChatSession)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ChatSessionId, e.CreatedAt });
        });

        // SearchLog Configuration
        modelBuilder.Entity<SearchLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SearchTerm).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.MinPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SortBy).HasMaxLength(50);
            entity.Property(e => e.SortOrder).HasMaxLength(20);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasIndex(e => e.SearchTerm);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // ProductQuestion Configuration
        modelBuilder.Entity<ProductQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ProductId, e.IsApproved });
            entity.HasIndex(e => e.UserId);
        });

        // ProductAnswer Configuration
        modelBuilder.Entity<ProductAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnswerText).IsRequired().HasMaxLength(2000);

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.QuestionId, e.IsApproved });
        });

        // QuestionVote Configuration
        modelBuilder.Entity<QuestionVote>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Votes)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.QuestionId, e.UserId }).IsUnique();
        });

        // AnswerVote Configuration
        modelBuilder.Entity<AnswerVote>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Answer)
                .WithMany(a => a.Votes)
                .HasForeignKey(e => e.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.AnswerId, e.UserId }).IsUnique();
        });

        // LoyaltyAccount Configuration
        modelBuilder.Entity<LoyaltyAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<LoyaltyAccount>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LoyaltyTransaction Configuration
        modelBuilder.Entity<LoyaltyTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.LoyaltyAccount)
                .WithMany(a => a.Transactions)
                .HasForeignKey(e => e.LoyaltyAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.LoyaltyAccountId, e.CreatedAt });
        });

        // LoyaltyReward Configuration
        modelBuilder.Entity<LoyaltyReward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
        });

        // RedeemedReward Configuration
        modelBuilder.Entity<RedeemedReward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RedemptionCode).IsUnique();
            entity.Property(e => e.RedemptionCode).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.LoyaltyAccount)
                .WithMany()
                .HasForeignKey(e => e.LoyaltyAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LoyaltyReward)
                .WithMany()
                .HasForeignKey(e => e.LoyaltyRewardId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.LoyaltyAccountId, e.IsUsed });
        });

        // Seed default currencies
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "USD", Symbol = "$", Name = "US Dollar", ExchangeRate = 1.000000m, IsActive = true },
            new Currency { Id = 2, Code = "EUR", Symbol = "€", Name = "Euro", ExchangeRate = 0.920000m, IsActive = true },
            new Currency { Id = 3, Code = "GBP", Symbol = "£", Name = "British Pound", ExchangeRate = 0.790000m, IsActive = true },
            new Currency { Id = 4, Code = "JPY", Symbol = "¥", Name = "Japanese Yen", ExchangeRate = 149.500000m, IsActive = true },
            new Currency { Id = 5, Code = "CAD", Symbol = "C$", Name = "Canadian Dollar", ExchangeRate = 1.360000m, IsActive = true },
            new Currency { Id = 6, Code = "AUD", Symbol = "A$", Name = "Australian Dollar", ExchangeRate = 1.530000m, IsActive = true },
            new Currency { Id = 7, Code = "CHF", Symbol = "CHF", Name = "Swiss Franc", ExchangeRate = 0.880000m, IsActive = true },
            new Currency { Id = 8, Code = "CNY", Symbol = "¥", Name = "Chinese Yuan", ExchangeRate = 7.250000m, IsActive = true }
        );
    }
}
