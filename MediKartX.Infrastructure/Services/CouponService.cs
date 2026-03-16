using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;
using System;

namespace MediKartX.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly MediKartXDbContext _db;

    public CouponService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<CouponDto?> GetByCodeAsync(string code)
    {
        var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
        if (c == null) return null;
        return new CouponDto { CouponId = c.CouponId, Code = c.Code, DiscountType = c.DiscountType, DiscountValue = c.DiscountValue ?? 0, ExpiryDate = c.ExpiryDate, MinOrderAmount = c.MinOrderAmount, MaxUsageCount = c.MaxUsageCount, IsActive = c.IsActive ?? true };
    }

    public async Task<ApplyCouponResult> ValidateAndApplyAsync(string code, int userId, decimal orderAmount)
    {
        var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
        if (c == null) return new ApplyCouponResult { Success = false, Message = "Coupon not found" };
        if (c.ExpiryDate.HasValue && c.ExpiryDate.Value < DateTime.UtcNow) return new ApplyCouponResult { Success = false, Message = "Coupon expired" };
        if (c.MinOrderAmount.HasValue && orderAmount < c.MinOrderAmount.Value) return new ApplyCouponResult { Success = false, Message = "Order amount too low" };

        // global usage count
        if (c.MaxUsageCount.HasValue)
        {
            var used = await _db.CouponUsages.CountAsync(u => u.CouponId == c.CouponId);
            if (used >= c.MaxUsageCount.Value) return new ApplyCouponResult { Success = false, Message = "Coupon usage limit reached" };
        }

        // user-wise usage
        var userUsed = await _db.CouponUsages.CountAsync(u => u.CouponId == c.CouponId && u.UserId == userId);
        if (c.MaxUsagePerUser.HasValue && userUsed >= c.MaxUsagePerUser.Value) return new ApplyCouponResult { Success = false, Message = "You have used this coupon maximum times" };

        decimal discount = 0;
        if (c.DiscountType == "Percentage")
            discount = Math.Round(orderAmount * ((c.DiscountValue ?? 0) / 100m), 2);
        else discount = c.DiscountValue ?? 0;

        // record usage in CouponUsage/CouponHistory
        var usage = new CouponUsage { CouponId = c.CouponId, UserId = userId, UsedAt = DateTime.UtcNow };
        _db.CouponUsages.Add(usage);
        await _db.SaveChangesAsync();

        return new ApplyCouponResult { Success = true, DiscountAmount = Math.Min(discount, orderAmount), Message = "Coupon applied" };
    }

    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        var exists = await _db.Coupons.AnyAsync(c => c.Code == dto.Code);
        if (exists) throw new Exception("Coupon code already exists");
        var entity = new Coupon
        {
            Code = dto.Code,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            ExpiryDate = dto.ExpiryDate,
            MinOrderAmount = dto.MinOrderAmount,
            MaxUsageCount = dto.MaxUsageCount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Coupons.Add(entity);
        await _db.SaveChangesAsync();
        return new CouponDto { CouponId = entity.CouponId, Code = entity.Code, DiscountType = entity.DiscountType, DiscountValue = entity.DiscountValue ?? 0, ExpiryDate = entity.ExpiryDate, MinOrderAmount = entity.MinOrderAmount, MaxUsageCount = entity.MaxUsageCount, IsActive = entity.IsActive ?? true };
    }
}
