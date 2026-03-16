using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface ICouponService
{
    Task<CouponDto?> GetByCodeAsync(string code);
    Task<ApplyCouponResult> ValidateAndApplyAsync(string code, int userId, decimal orderAmount);
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
}
