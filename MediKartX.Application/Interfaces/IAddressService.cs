using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;
public interface IAddressService
{
    Task<List<AddressDto>> GetAsync(int userId);
    Task<AddressDto> AddAsync(int userId, AddressRequest req);
    Task<bool> DeleteAsync(int addressId, int userId);
}