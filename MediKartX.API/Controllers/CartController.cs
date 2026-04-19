using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // ✅ COMMON METHOD
    private int? GetUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (claim == null || !int.TryParse(claim.Value, out int userId))
            return null;

        return userId;
    }

    // ================= GET CART =================
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? guestToken = null)
    {
        var userId = GetUserId();

        var cart = await _cartService.GetCartAsync(guestToken, userId);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = cart,
            Errors = null,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ================= MERGE CART =================
    [Authorize]
    [HttpPost("merge")]
    public async Task<IActionResult> Merge([FromBody] string guestToken)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid token",
                Data = null,
                Errors = new[] { "Unauthorized" },
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            });
        }

        var result = await _cartService.MergeGuestCartAsync(guestToken, userId.Value);

        if (!result.ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.error,
                Data = null,
                Errors = new[] { result.error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Guest cart merged successfully",
            Data = null,
            Errors = null,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ================= REMOVE ITEM =================
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> Remove(int cartItemId, [FromQuery] string? guestToken = null)
    {
        var userId = GetUserId();

        var (ok, error, cart) = await _cartService.RemoveItemAsync(cartItemId, guestToken, userId);

        if (!ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = error,
                Data = null,
                Errors = new[] { error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Item removed successfully",
            Data = cart,
            Errors = null,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ================= ADD TO CART =================
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest req)
    {
        if (req == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid request",
                Data = null,
                Errors = new[] { "Request body is required" },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        var userId = GetUserId();

        if (userId != null)
        {
            req.UserId = userId.Value;
        }

        var result = await _cartService.AddToCartAsync(req);

        if (!result.ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to add to cart",
                Data = null,
                Errors = new[] { result.error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Cart updated successfully",
            Data = result.cart,
            Errors = null,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }
}