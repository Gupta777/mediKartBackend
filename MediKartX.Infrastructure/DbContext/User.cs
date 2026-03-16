using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class User
{
    public int UserId { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string? Email { get; set; }

    public bool? IsMobileVerified { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Otplog> Otplogs { get; set; } = new List<Otplog>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
