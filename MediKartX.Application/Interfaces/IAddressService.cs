using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;
public interface IAddressService
{
    Task<List<AddressDto>> GetUserAddressesAsync(int userId);
    Task<AddressDto?> AddAddressAsync(int userId, AddAddressRequest req);
    Task<bool> UpdateAddressAsync(int userId, UpdateAddressRequest req);
    Task<bool> DeleteAddressAsync(int userId, int addressId);
    Task<bool> SetDefaultAddressAsync(int userId, int addressId);
}