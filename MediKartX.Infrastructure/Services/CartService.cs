using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;
using System;

namespace MediKartX.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly MediKartXDbContext _db;

    public CartService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(string? guestToken, int? userId)
    {
        Cart? cart = null;
        if (!string.IsNullOrWhiteSpace(guestToken))
            cart = await _db.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Medicine).FirstOrDefaultAsync(c => c.GuestToken == guestToken);
        if (cart == null && userId.HasValue)
            cart = await _db.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Medicine).FirstOrDefaultAsync(c => c.UserId == userId.Value);
        if (cart == null)
        {
            return new CartDto { Items = new System.Collections.Generic.List<CartItemDto>(), SubTotal = 0, Total = 0 };
        }

        var dto = new CartDto
        {
            CartId = cart.CartId,
            GuestToken = cart.GuestToken,
            UserId = cart.UserId == 0 ? null : cart.UserId,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                CartItemId = ci.CartItemId,
                MedicineId = ci.MedicineId,
                MedicineName = ci.Medicine?.Name,
                Price = ci.Medicine?.SellingPrice ?? 0,
                Quantity = ci.Quantity
            }).ToList()
        };
        dto.SubTotal = dto.Items.Sum(i => i.LineTotal);
        dto.Total = dto.SubTotal;
        return dto;
    }

   public async Task<(bool ok, string? error, CartDto? cart)> AddToCartAsync(AddToCartRequest req)
{
    // STEP 1: Get Medicine
    var med = await _db.Medicines.FirstOrDefaultAsync(m => m.MedicineId == req.MedicineId);

    if (med == null) return (false, "Medicine not found", null);
    if ((bool)!med.IsActive) return (false, "Medicine is inactive", null);
    if (med.Stock < req.Quantity) return (false, "Insufficient stock", null);

    Cart? cart = null;

    // STEP 2: Logged-in user cart
    if (req.UserId.HasValue)
    {
        cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId.Value);
    }
    // STEP 3: Guest cart
    else if (!string.IsNullOrWhiteSpace(req.GuestToken))
    {
        cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.GuestToken == req.GuestToken);
    }

    // STEP 4: Create cart if not exists
    if (cart == null)
    {
        cart = new Cart
        {
            UserId = req.UserId??0,
            GuestToken = req.UserId.HasValue ? null : req.GuestToken,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CartItems = new List<CartItem>()
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
    }

    // STEP 5: Add or update item
    var existing = cart.CartItems.FirstOrDefault(x => x.MedicineId == req.MedicineId);

    if (existing != null)
    {
        var newQty = existing.Quantity + req.Quantity;

        if (med.Stock < newQty)
            return (false, "Insufficient stock for combined quantity", null);

        existing.Quantity = newQty;
        existing.UpdatedAt = DateTime.UtcNow;
    }
    else
    {
        cart.CartItems.Add(new CartItem
        {
            MedicineId = req.MedicineId,
            Quantity = req.Quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    cart.UpdatedAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    var dto = await GetCartAsync(req.GuestToken, req.UserId);

    return (true, null, dto);
}
 public async Task<(bool ok, string? error, CartDto? cart)> RemoveItemAsync(int cartItemId, string? guestToken, int? userId)
    {
        CartItem? item = null;
        if (!string.IsNullOrWhiteSpace(guestToken))
            item = await _db.CartItems.Include(ci => ci.Cart).FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.GuestToken == guestToken);
        if (item == null && userId.HasValue)
            item = await _db.CartItems.Include(ci => ci.Cart).FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId.Value);
        if (item == null) return (false, "Cart item not found", null);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        var dto = await GetCartAsync(guestToken, userId);
        return (true, null, dto);
    }

 public async Task<(bool ok, string? error)> MergeGuestCartAsync(string guestToken, int userId)
{
    var guestCart = await _db.Carts
        .Include(c => c.CartItems)
        .FirstOrDefaultAsync(c => c.GuestToken == guestToken);

    if (guestCart == null)
        return (false, "Guest cart not found");

    var userCart = await _db.Carts
        .Include(c => c.CartItems)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    if (userCart == null)
    {
        userCart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CartItems = new List<CartItem>()
        };

        _db.Carts.Add(userCart);
        await _db.SaveChangesAsync();
    }

    // STEP 1: Merge items
    foreach (var item in guestCart.CartItems.ToList())
    {
        var existing = userCart.CartItems
            .FirstOrDefault(x => x.MedicineId == item.MedicineId);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            userCart.CartItems.Add(new CartItem
            {
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _db.CartItems.Remove(item);
    }

    // STEP 2: delete guest cart
    _db.Carts.Remove(guestCart);

    await _db.SaveChangesAsync();

    return (true, null);
}
}
