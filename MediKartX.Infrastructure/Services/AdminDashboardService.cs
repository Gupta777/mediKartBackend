using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly MediKartXDbContext _db;
    private readonly IMemoryCache _cache;

    public AdminDashboardService(MediKartXDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<object>> GetSummaryAsync()
    {
        var cacheKey = "admin_dashboard_summary";
        if (!_cache.TryGetValue(cacheKey, out object? cached))
        {
            var totalOrders = await _db.Orders.CountAsync();
            var totalRevenue = await _db.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var activeUsers = await _db.Users.CountAsync(u => u.IsActive == true);

            var topSelling = await _db.OrderItems
                .Include(oi => oi.Medicine)
                .GroupBy(oi => new { oi.MedicineId, oi.Medicine!.Name })
                .Select(g => new { MedicineId = g.Key.MedicineId, Name = g.Key.Name, Quantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToListAsync();

            var lowStock = await _db.Medicines
                .Where(m => m.Stock <= 5 && (m.IsActive == true || m.IsActive == null))
                .Select(m => new { m.MedicineId, m.Name, m.Stock })
                .ToListAsync();

            var summary = new { totalOrders, totalRevenue, activeUsers, topSelling, lowStock };
            _cache.Set(cacheKey, summary, TimeSpan.FromMinutes(5));
            cached = summary;
        }

        return new ApiResponse<object> { Success = true, Message = "Dashboard summary", Data = cached, StatusCode = 200 };
    }

    public async Task<ApiResponse<object>> GetMonthlySalesAsync(int months)
    {
        var now = DateTime.UtcNow;
        var from = now.AddMonths(-months + 1);
        var data = await _db.Orders
            .Where(o => o.CreatedAt >= from)
            .GroupBy(o => new { Year = o.CreatedAt.Value.Year, Month = o.CreatedAt.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return new ApiResponse<object> { Success = true, Message = "Monthly sales", Data = data, StatusCode = 200 };
    }
}
