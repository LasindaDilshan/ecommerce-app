using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestCartFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Carts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AbandonedCarts",
                columns: table => new
                {
                    AbandonedCartId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    GuestEmail = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    AbandonedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CartValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    CartItemsJson = table.Column<string>(type: "text", nullable: true),
                    RecoveryEmailsSent = table.Column<int>(type: "integer", nullable: false),
                    LastEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRecovered = table.Column<bool>(type: "boolean", nullable: false),
                    RecoveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecoveredOrderId = table.Column<int>(type: "integer", nullable: true),
                    RecoveryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbandonedCarts", x => x.AbandonedCartId);
                    table.ForeignKey(
                        name: "FK_AbandonedCarts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GiftCards",
                columns: table => new
                {
                    GiftCardId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InitialValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PurchasedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RecipientUserId = table.Column<int>(type: "integer", nullable: true),
                    RecipientEmail = table.Column<string>(type: "text", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.GiftCardId);
                    table.ForeignKey(
                        name: "FK_GiftCards_Users_PurchasedByUserId",
                        column: x => x.PurchasedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GiftCards_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequests",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ReturnLabelUrl = table.Column<string>(type: "text", nullable: true),
                    TrackingNumber = table.Column<string>(type: "text", nullable: true),
                    RestockingFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RefundMethod = table.Column<int>(type: "integer", nullable: false),
                    RefundProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundTransactionId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequests", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BillingInterval = table.Column<int>(type: "integer", nullable: false),
                    BillingIntervalCount = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SetupFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TrialPeriodDays = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    FeaturesJson = table.Column<string>(type: "text", nullable: true),
                    StripePriceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "GiftCardTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GiftCardId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCardTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_GiftCards_GiftCardId",
                        column: x => x.GiftCardId,
                        principalTable: "GiftCards",
                        principalColumn: "GiftCardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiftCardTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnItems",
                columns: table => new
                {
                    ReturnItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnItems", x => x.ReturnItemId);
                    table.ForeignKey(
                        name: "FK_ReturnItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnItems_ReturnRequests_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "ReturnRequests",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanProducts",
                columns: table => new
                {
                    PlanProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanProducts", x => x.PlanProductId);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanProducts_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PausedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    PaymentMethodId = table.Column<string>(type: "text", nullable: true),
                    CardLast4 = table.Column<string>(type: "text", nullable: true),
                    CardBrand = table.Column<string>(type: "text", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiftSubscriptions",
                columns: table => new
                {
                    GiftSubscriptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    GiverUserId = table.Column<int>(type: "integer", nullable: false),
                    RecipientUserId = table.Column<int>(type: "integer", nullable: false),
                    GiftMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRedeemed = table.Column<bool>(type: "boolean", nullable: false),
                    RedemptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftSubscriptions", x => x.GiftSubscriptionId);
                    table.ForeignKey(
                        name: "FK_GiftSubscriptions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiftSubscriptions_Users_GiverUserId",
                        column: x => x.GiverUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiftSubscriptions_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionModifications",
                columns: table => new
                {
                    ModificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OldPlanId = table.Column<int>(type: "integer", nullable: true),
                    NewPlanId = table.Column<int>(type: "integer", nullable: true),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NewPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionModifications", x => x.ModificationId);
                    table.ForeignKey(
                        name: "FK_SubscriptionModifications_SubscriptionPlans_NewPlanId",
                        column: x => x.NewPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId");
                    table.ForeignKey(
                        name: "FK_SubscriptionModifications_SubscriptionPlans_OldPlanId",
                        column: x => x.OldPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId");
                    table.ForeignKey(
                        name: "FK_SubscriptionModifications_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionModifications_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionOrders",
                columns: table => new
                {
                    SubscriptionOrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShippedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    SkipReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionOrders", x => x.SubscriptionOrderId);
                    table.ForeignKey(
                        name: "FK_SubscriptionOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionOrders_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    StripeInvoiceId = table.Column<string>(type: "text", nullable: true),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedCarts_UserId",
                table: "AbandonedCarts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_PurchasedByUserId",
                table: "GiftCards",
                column: "PurchasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_RecipientUserId",
                table: "GiftCards",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_GiftCardId",
                table: "GiftCardTransactions",
                column: "GiftCardId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardTransactions_OrderId",
                table: "GiftCardTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftSubscriptions_GiverUserId",
                table: "GiftSubscriptions",
                column: "GiverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftSubscriptions_RecipientUserId",
                table: "GiftSubscriptions",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftSubscriptions_SubscriptionId",
                table: "GiftSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ProductId",
                table: "ReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ReturnId",
                table: "ReturnItems",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_ApprovedByUserId",
                table: "ReturnRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_OrderId",
                table: "ReturnRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_UserId",
                table: "ReturnRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModifications_ModifiedByUserId",
                table: "SubscriptionModifications",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModifications_NewPlanId",
                table: "SubscriptionModifications",
                column: "NewPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModifications_OldPlanId",
                table: "SubscriptionModifications",
                column: "OldPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModifications_SubscriptionId",
                table: "SubscriptionModifications",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_OrderId",
                table: "SubscriptionOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrders_SubscriptionId",
                table: "SubscriptionOrders",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanProducts_PlanId",
                table: "SubscriptionPlanProducts",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanProducts_ProductId",
                table: "SubscriptionPlanProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ShippingAddressId",
                table: "Subscriptions",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbandonedCarts");

            migrationBuilder.DropTable(
                name: "GiftCardTransactions");

            migrationBuilder.DropTable(
                name: "GiftSubscriptions");

            migrationBuilder.DropTable(
                name: "ReturnItems");

            migrationBuilder.DropTable(
                name: "SubscriptionModifications");

            migrationBuilder.DropTable(
                name: "SubscriptionOrders");

            migrationBuilder.DropTable(
                name: "SubscriptionPayments");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanProducts");

            migrationBuilder.DropTable(
                name: "GiftCards");

            migrationBuilder.DropTable(
                name: "ReturnRequests");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Carts");
        }
    }
}
