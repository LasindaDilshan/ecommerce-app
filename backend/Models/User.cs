using Backend.Models;

namespace EcommerceAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "User"; // "Admin" or "User"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public DateTime? TwoFactorEnabledAt { get; set; }
    public string? RecoveryCodes { get; set; } // JSON serialized list of recovery codes
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }

    // Email Verification
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpires { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual Cart? Cart { get; set; }
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual RefreshToken? RefreshToken { get; set; }
    public virtual Wishlist? Wishlist { get; set; }
}
