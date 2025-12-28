using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    // Subscription Plan (Template for subscriptions)
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public SubscriptionInterval BillingInterval { get; set; }

        public int BillingIntervalCount { get; set; } = 1; // e.g., every 2 months

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SetupFee { get; set; }

        public int? TrialPeriodDays { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVisible { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        // Features JSON (could be a separate table for complex scenarios)
        public string? FeaturesJson { get; set; }

        // Stripe Price ID for integration
        public string? StripePriceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<SubscriptionPlanProduct> PlanProducts { get; set; } = new List<SubscriptionPlanProduct>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }

    // Products included in a subscription plan
    public class SubscriptionPlanProduct
    {
        [Key]
        public int PlanProductId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [ForeignKey("PlanId")]
        public SubscriptionPlan Plan { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        public bool IsOptional { get; set; } = false;
    }

    // User Subscription
    public class Subscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        [StringLength(50)]
        public string SubscriptionNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public int PlanId { get; set; }

        [ForeignKey("PlanId")]
        public SubscriptionPlan Plan { get; set; } = null!;

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? TrialEndDate { get; set; }

        public DateTime NextBillingDate { get; set; }

        public DateTime? PausedUntil { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancellationReason { get; set; }

        // Payment Method
        public string? StripeSubscriptionId { get; set; }

        public string? StripeCustomerId { get; set; }

        public string? PaymentMethodId { get; set; }

        public string? CardLast4 { get; set; }

        public string? CardBrand { get; set; }

        // Shipping Address
        public int? ShippingAddressId { get; set; }

        [ForeignKey("ShippingAddressId")]
        public Address? ShippingAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
        public ICollection<SubscriptionOrder> Orders { get; set; } = new List<SubscriptionOrder>();
        public ICollection<SubscriptionModification> Modifications { get; set; } = new List<SubscriptionModification>();
    }

    // Subscription Payment History
    public class SubscriptionPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public string? StripePaymentIntentId { get; set; }

        public string? StripeInvoiceId { get; set; }

        public int? InvoiceId { get; set; }

        public int RetryCount { get; set; } = 0;

        public DateTime? NextRetryDate { get; set; }

        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Subscription Orders (Generated from subscriptions)
    public class SubscriptionOrder
    {
        [Key]
        public int SubscriptionOrderId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = null!;

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        public DateTime ScheduledDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public bool IsSkipped { get; set; } = false;

        public string? SkipReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Subscription Modifications (upgrades, downgrades, pauses)
    public class SubscriptionModification
    {
        [Key]
        public int ModificationId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = null!;

        public SubscriptionModificationType Type { get; set; }

        public int? OldPlanId { get; set; }

        [ForeignKey("OldPlanId")]
        public SubscriptionPlan? OldPlan { get; set; }

        public int? NewPlanId { get; set; }

        [ForeignKey("NewPlanId")]
        public SubscriptionPlan? NewPlan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? NewPrice { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string? Reason { get; set; }

        public int? ModifiedByUserId { get; set; }

        [ForeignKey("ModifiedByUserId")]
        public User? ModifiedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Gift Subscription
    public class GiftSubscription
    {
        [Key]
        public int GiftSubscriptionId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = null!;

        [Required]
        public int GiverUserId { get; set; }

        [ForeignKey("GiverUserId")]
        public User Giver { get; set; } = null!;

        [Required]
        public int RecipientUserId { get; set; }

        [ForeignKey("RecipientUserId")]
        public User Recipient { get; set; } = null!;

        [StringLength(500)]
        public string? GiftMessage { get; set; }

        public DateTime? RedemptionDate { get; set; }

        public bool IsRedeemed { get; set; } = false;

        [StringLength(50)]
        public string RedemptionCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Returns & RMA System
    public class ReturnRequest
    {
        [Key]
        public int ReturnId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReturnNumber { get; set; } = string.Empty;

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        public ReturnReason Reason { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public int? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }

        public string? ReturnLabelUrl { get; set; }

        public string? TrackingNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RestockingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        public RefundMethod RefundMethod { get; set; } = RefundMethod.OriginalPayment;

        public DateTime? RefundProcessedDate { get; set; }

        public string? RefundTransactionId { get; set; }

        // Navigation properties
        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }

    // Return Items
    public class ReturnItem
    {
        [Key]
        public int ReturnItemId { get; set; }

        [Required]
        public int ReturnId { get; set; }

        [ForeignKey("ReturnId")]
        public ReturnRequest Return { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        public ReturnItemCondition Condition { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Abandoned Cart Recovery
    public class AbandonedCart
    {
        [Key]
        public int AbandonedCartId { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [EmailAddress]
        public string? GuestEmail { get; set; }

        public string? SessionId { get; set; }

        public DateTime AbandonedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CartValue { get; set; }

        public int ItemCount { get; set; }

        public string? CartItemsJson { get; set; }

        public int RecoveryEmailsSent { get; set; } = 0;

        public DateTime? LastEmailSentAt { get; set; }

        public bool IsRecovered { get; set; } = false;

        public DateTime? RecoveredAt { get; set; }

        public int? RecoveredOrderId { get; set; }

        [StringLength(50)]
        public string? RecoveryCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Gift Cards
    public class GiftCard
    {
        [Key]
        public int GiftCardId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public int? PurchasedByUserId { get; set; }

        [ForeignKey("PurchasedByUserId")]
        public User? PurchasedBy { get; set; }

        public int? RecipientUserId { get; set; }

        [ForeignKey("RecipientUserId")]
        public User? Recipient { get; set; }

        [EmailAddress]
        public string? RecipientEmail { get; set; }

        [StringLength(100)]
        public string? RecipientName { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ActivatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<GiftCardTransaction> Transactions { get; set; } = new List<GiftCardTransaction>();
    }

    // Gift Card Transactions
    public class GiftCardTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int GiftCardId { get; set; }

        [ForeignKey("GiftCardId")]
        public GiftCard GiftCard { get; set; } = null!;

        public GiftCardTransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }

    // Enums
    public enum SubscriptionInterval
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }

    public enum SubscriptionStatus
    {
        Active,
        Trialing,
        PastDue,
        Paused,
        Cancelled,
        Expired
    }

    public enum SubscriptionModificationType
    {
        Upgrade,
        Downgrade,
        Pause,
        Resume,
        Cancel,
        Reactivate,
        PriceChange,
        AddressChange
    }

    public enum ReturnStatus
    {
        Pending,
        Approved,
        Rejected,
        Shipped,
        Received,
        Processing,
        Completed,
        Cancelled
    }

    public enum ReturnReason
    {
        Defective,
        WrongItem,
        NotAsDescribed,
        NoLongerNeeded,
        BetterPriceAvailable,
        Damaged,
        Other
    }

    public enum ReturnItemCondition
    {
        Unopened,
        Opened,
        Used,
        Damaged,
        Missing
    }

    public enum RefundMethod
    {
        OriginalPayment,
        StoreCredit,
        GiftCard,
        BankTransfer,
        Check
    }

    public enum GiftCardTransactionType
    {
        Purchase,
        Redemption,
        Refund,
        Adjustment,
        Expiration
    }
}