using System.Threading.Tasks;
using MediKartX.Application.DTOs;
using System.IO;

namespace MediKartX.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string? guestToken, int? userId);
    Task<(bool ok, string? error, CartDto? cart)> AddToCartAsync(AddToCartRequest req);
    Task<(bool ok, string? error, CartDto? cart)> RemoveItemAsync(int cartItemId, string? guestToken, int? userId);
    Task<(bool ok, string? error)> MergeGuestCartAsync(string guestToken, int userId);
}
