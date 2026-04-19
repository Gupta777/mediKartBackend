using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _svc;

    public WishlistController(IWishlistService svc)
    {
        _svc = svc;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }

    // ✅ GET WISHLIST
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();

        var data = await _svc.GetWishlistAsync(userId);

        return Ok(new ApiResponse<WishlistDto>
        {
            Success = true,
            Message = "Wishlist retrieved",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ✅ ADD ITEM
    [HttpPost("{medicineId}")]
    public async Task<IActionResult> Add(int medicineId)
    {
        var userId = GetUserId();

        var result = await _svc.AddAsync(userId, medicineId);

        if (!result.ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.error,
                Errors = new[] { result.error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<WishlistDto>
        {
            Success = true,
            Message = "Added to wishlist",
            Data = result.wishlist,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ✅ REMOVE ITEM
    [HttpDelete("{medicineId}")]
    public async Task<IActionResult> Remove(int medicineId)
    {
        var userId = GetUserId();

        var result = await _svc.RemoveAsync(userId, medicineId);

        if (!result.ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.error,
                Errors = new[] { result.error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<WishlistDto>
        {
            Success = true,
            Message = "Removed from wishlist",
            Data = result.wishlist,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("move-to-cart/{medicineId}")]
public async Task<IActionResult> MoveToCart(int medicineId)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

    var result = await _svc.MoveToCartAsync(userId, medicineId);

    if (!result.ok)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = result.error,
            Errors = new[] { result.error },
            StatusCode = 400,
            Timestamp = DateTime.UtcNow
        });
    }

    return Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Moved to cart successfully",
        Data = null,
        StatusCode = 200,
        Timestamp = DateTime.UtcNow
    });
}
}