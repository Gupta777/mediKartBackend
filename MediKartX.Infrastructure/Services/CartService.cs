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
        // Resolve cart
        Cart? cart = null;
        if (!string.IsNullOrWhiteSpace(req.GuestToken))
            cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.GuestToken == req.GuestToken);
        if (cart == null && req.UserId.HasValue)
            cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == req.UserId.Value);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = req.UserId ?? 0,
                GuestToken = req.GuestToken,
                CreatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        var med = await _db.Medicines.FirstOrDefaultAsync(m => m.MedicineId == req.MedicineId);
        if (med == null) return (false, "Medicine not found", null);
        if (med.IsActive == false) return (false, "Medicine is inactive", null);
        if (med.Stock < req.Quantity) return (false, "Insufficient stock", null);

        var existing = cart.CartItems.FirstOrDefault(ci => ci.MedicineId == req.MedicineId);
        if (existing != null)
        {
            var newQty = existing.Quantity + req.Quantity;
            if (med.Stock < newQty) return (false, "Insufficient stock for combined quantity", null);
            existing.Quantity = newQty;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                MedicineId = req.MedicineId,
                Quantity = req.Quantity,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        var dto = await GetCartAsync(cart.GuestToken, cart.UserId == 0 ? (int?)null : cart.UserId);
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
        if (string.IsNullOrWhiteSpace(guestToken)) return (false, "guestToken required");
        var guestCart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.GuestToken == guestToken);
        if (guestCart == null) return (false, "Guest cart not found");
        var userCart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (userCart == null)
        {
            userCart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _db.Carts.Add(userCart);
            await _db.SaveChangesAsync();
        }

        foreach (var gi in guestCart.CartItems.ToList())
        {
            var med = await _db.Medicines.FindAsync(gi.MedicineId);
            if (med == null || med.IsActive == false) continue; // skip
            var existing = userCart.CartItems.FirstOrDefault(ci => ci.MedicineId == gi.MedicineId);
            if (existing != null)
            {
                var combined = existing.Quantity + gi.Quantity;
                if (med.Stock < combined)
                {
                    existing.Quantity = Math.Min(med.Stock, combined);
                }
                else existing.Quantity = combined;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var qty = Math.Min(gi.Quantity, med.Stock);
                userCart.CartItems.Add(new CartItem { MedicineId = gi.MedicineId, Quantity = qty, CreatedAt = DateTime.UtcNow });
            }
            // remove guest item
            _db.CartItems.Remove(gi);
        }

        // remove guest cart
        _db.Carts.Remove(guestCart);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
