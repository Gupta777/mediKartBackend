using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<ApiResponse<object>> GetSummaryAsync();
    Task<ApiResponse<object>> GetMonthlySalesAsync(int months);
}
