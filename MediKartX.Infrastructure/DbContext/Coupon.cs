using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string Code { get; set; } = null!;

    // Flexible discount model: type can be 'Percentage' or 'Fixed'
    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    // kept for backward compatibility (legacy percent field)
    public int DiscountPercent { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public int? MaxUsageCount { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}
