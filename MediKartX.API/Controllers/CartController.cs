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

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? guestToken = null)
    {
        int? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsed))
                userId = parsed;
        }

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
    [HttpPost("merge")]
    [Authorize]
    public async Task<IActionResult> Merge([FromBody] string guestToken)
    {
        var userId = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        );

        var result = await _cartService.MergeGuestCartAsync(guestToken, userId);

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
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> Remove(int cartItemId, [FromQuery] string? guestToken = null)
    {
        int? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsed))
                userId = parsed;
        }

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



    [HttpPost("add")]
    public async Task<IActionResult> Add(AddToCartRequest req)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            );
            req.UserId = userId;
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
