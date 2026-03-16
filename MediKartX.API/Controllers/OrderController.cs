using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _svc;

    public OrderController(IOrderService svc)
    {
        _svc = svc;
    }

    [HttpPost("place")]
    [Authorize]
    public async Task<IActionResult> Place([FromBody] PlaceOrderRequest req)
    {
        // enforce user from token
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId)) return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid user", Errors = new[] { "Invalid user" }, StatusCode = 400 });
        req.UserId = userId;

        var (ok, error, order) = await _svc.PlaceOrderAsync(req);
        if (!ok) return BadRequest(new ApiResponse<object> { Success = false, Message = error, Errors = new[] { error }, StatusCode = 400 });
        return Ok(new ApiResponse<OrderDto> { Success = true, Message = "Order placed", Data = order, StatusCode = 200 });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var order = await _svc.GetByIdAsync(id);
        if (order == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Order not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<OrderDto> { Success = true, Message = "Order retrieved", Data = order, StatusCode = 200 });
    }

    [HttpGet("{id}/history")]
    [Authorize]
    public async Task<IActionResult> History(int id)
    {
        var hist = await _svc.GetOrderHistoryAsync(id);
        return Ok(new ApiResponse<OrderHistoryDto[]> { Success = true, Message = "Order history", Data = hist, StatusCode = 200 });
    }

    [HttpPost("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
    {
        var (ok, error) = await _svc.UpdateOrderStatusAsync(id, status);
        if (!ok) return BadRequest(new ApiResponse<object> { Success = false, Message = error, Errors = new[] { error ?? "failed" }, StatusCode = 400 });
        return Ok(new ApiResponse<object> { Success = true, Message = "Order status updated", StatusCode = 200 });
    }
}
