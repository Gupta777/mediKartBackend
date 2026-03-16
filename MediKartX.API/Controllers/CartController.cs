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
    public async Task<ActionResult<CartDto>> Get([FromQuery] string? guestToken = null)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsed)) userId = parsed;
        }

        var cart = await _cartService.GetCartAsync(guestToken, userId);
        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<ActionResult> Add([FromBody] AddToCartRequest req)
    {
        // if authenticated, prefer userId from token
        if (User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsed)) req.UserId = parsed;
        }

        var (ok, error, cart) = await _cartService.AddToCartAsync(req);
        if (!ok) return BadRequest(new { success = false, error });
        return Ok(cart);
    }

    [HttpPost("merge")]
    [Authorize]
    public async Task<ActionResult> Merge([FromBody] MergeCartRequest req)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId)) return BadRequest(new { success = false, error = "Invalid user" });

        var (ok, error) = await _cartService.MergeGuestCartAsync(req.GuestToken, userId);
        if (!ok) return BadRequest(new { success = false, error });
        return Ok(new { success = true });
    }

    [HttpDelete("item/{cartItemId}")]
    public async Task<ActionResult> Remove(int cartItemId, [FromQuery] string? guestToken = null)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsed)) userId = parsed;
        }

        var (ok, error, cart) = await _cartService.RemoveItemAsync(cartItemId, guestToken, userId);
        if (!ok) return BadRequest(new { success = false, error });
        return Ok(cart);
    }
}
