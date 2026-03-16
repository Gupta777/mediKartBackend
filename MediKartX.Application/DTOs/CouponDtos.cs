using System;

namespace MediKartX.Application.DTOs;

public class CouponDto
{
    public int CouponId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Flat"; // Flat or Percentage
    public decimal DiscountValue { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Flat";
    public decimal DiscountValue { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
}

public class ApplyCouponResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
}
