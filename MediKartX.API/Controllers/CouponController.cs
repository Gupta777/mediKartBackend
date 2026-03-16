using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _svc;

    public CouponController(ICouponService svc)
    {
        _svc = svc;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        try
        {
            var res = await _svc.CreateAsync(dto);
            return Ok(new ApiResponse<CouponDto> { Success = true, Message = "Coupon created", Data = res, StatusCode = 200 });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Create failed", Errors = new[] { ex.Message }, StatusCode = 400 });
        }
    }

    [HttpGet("apply/{code}")]
    [Authorize]
    public async Task<IActionResult> Apply(string code, [FromQuery] decimal orderAmount)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId)) return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid user", Errors = new[] { "Invalid user" }, StatusCode = 400 });
        var res = await _svc.ValidateAndApplyAsync(code, userId, orderAmount);
        if (!res.Success) return BadRequest(new ApiResponse<object> { Success = false, Message = res.Message, Errors = new[] { res.Message }, StatusCode = 400 });
        return Ok(new ApiResponse<object> { Success = true, Message = "Coupon applied", Data = new { discount = res.DiscountAmount }, StatusCode = 200 });
    }
}
