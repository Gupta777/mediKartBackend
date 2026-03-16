using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using System.Threading.Tasks;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _svc;

    public AdminDashboardController(IAdminDashboardService svc)
    {
        _svc = svc;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var res = await _svc.GetSummaryAsync();
        return Ok(res);
    }

    [HttpGet("monthly-sales")]
    public async Task<IActionResult> MonthlySales([FromQuery] int months = 6)
    {
        var res = await _svc.GetMonthlySalesAsync(months);
        return Ok(res);
    }
}
