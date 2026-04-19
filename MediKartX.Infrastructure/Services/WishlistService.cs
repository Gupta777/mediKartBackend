using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.DTOs;
using MediKartX.Application.Interfaces;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;


public class WishlistService : IWishlistService
{
    private readonly MediKartXDbContext _db;

    public WishlistService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<WishlistDto> GetWishlistAsync(int userId)
    {
        var wishlist = await _db.Wishlists
            .Include(w => w.WishlistItems)
            .ThenInclude(wi => wi.Medicine)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            return new WishlistDto
            {
                UserId = userId,
                Items = new()
            };
        }

        return new WishlistDto
        {
            WishlistId = wishlist.WishlistId,
            UserId = wishlist.UserId,
            Items = wishlist.WishlistItems.Select(wi => new WishlistItemDto
            {
                WishlistItemId = wi.WishlistItemId,
                MedicineId = wi.MedicineId,
                MedicineName = wi.Medicine.Name,
                Price = wi.Medicine.SellingPrice,
                ImageUrl = wi.Medicine.ImageUrl
            }).ToList()
        };
    }

    public async Task<(bool ok, string? error, WishlistDto? wishlist)> AddAsync(int userId, int medicineId)
    {
        var med = await _db.Medicines.FirstOrDefaultAsync(m => m.MedicineId == medicineId);

        if (med == null)
            return (false, "Medicine not found", null);

        var wishlist = await _db.Wishlists
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                UserId = userId,
                WishlistItems = new List<WishlistItem>()
            };

            _db.Wishlists.Add(wishlist);
            await _db.SaveChangesAsync();
        }

        var exists = wishlist.WishlistItems
            .Any(x => x.MedicineId == medicineId);

        if (exists)
            return (false, "Already in wishlist", null);

        wishlist.WishlistItems.Add(new WishlistItem
        {
            MedicineId = medicineId
        });

        await _db.SaveChangesAsync();

        var dto = await GetWishlistAsync(userId);
        return (true, null, dto);
    }

    public async Task<(bool ok, string? error, WishlistDto? wishlist)> RemoveAsync(int userId, int medicineId)
    {
        var wishlist = await _db.Wishlists
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
            return (false, "Wishlist not found", null);

        var item = wishlist.WishlistItems
            .FirstOrDefault(x => x.MedicineId == medicineId);

        if (item == null)
            return (false, "Item not found", null);

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();

        var dto = await GetWishlistAsync(userId);
        return (true, null, dto);
    }

    public async Task<(bool ok, string? error)> MoveToCartAsync(int userId, int medicineId)
{
    // 1. Get wishlist
    var wishlist = await _db.Wishlists
        .Include(w => w.WishlistItems)
        .FirstOrDefaultAsync(w => w.UserId == userId);

    if (wishlist == null)
        return (false, "Wishlist not found");

    var item = wishlist.WishlistItems
        .FirstOrDefault(x => x.MedicineId == medicineId);

    if (item == null)
        return (false, "Item not found in wishlist");

    // 2. Get/Create cart
    var cart = await _db.Carts
        .Include(c => c.CartItems)
        .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null)
    {
        cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CartItems = new List<CartItem>()
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
    }

    // 3. Add to cart
    var existing = cart.CartItems
        .FirstOrDefault(x => x.MedicineId == medicineId);

    if (existing != null)
    {
        existing.Quantity += 1;
        existing.UpdatedAt = DateTime.UtcNow;
    }
    else
    {
        cart.CartItems.Add(new CartItem
        {
            MedicineId = medicineId,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    // 4. Remove from wishlist
    _db.WishlistItems.Remove(item);

    await _db.SaveChangesAsync();

    return (true, null);
}
}