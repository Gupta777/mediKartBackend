using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;


public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(int userId);

    Task<(bool ok, string? error, WishlistDto? wishlist)> AddAsync(int userId, int medicineId);

    Task<(bool ok, string? error, WishlistDto? wishlist)> RemoveAsync(int userId, int medicineId);
    Task<(bool ok, string? error)> MoveToCartAsync(int userId, int medicineId);
}