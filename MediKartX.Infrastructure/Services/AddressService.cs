using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly MediKartXDbContext _db;

    public AddressService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<List<AddressDto>> GetAsync(int userId)
    {
        return await _db.Addresses
            .Where(x => x.UserId == userId)
            .Select(x => new AddressDto
            {
                AddressId = x.AddressId,
                AddressLine = x.AddressLine,
                City = x.City,
                State = x.State,
                Pincode = x.Pincode,
                IsDefault = (bool)x.IsDefault,
                AddressType = x.AddressType
            }).ToListAsync();
    }

    public async Task<AddressDto> AddAsync(int userId, AddressRequest req)
    {
        if (req.IsDefault)
        {
            var existing = _db.Addresses.Where(x => x.UserId == userId);
            foreach (var addr in existing)
                addr.IsDefault = false;
        }

        var address = new Address
        {
            UserId = userId,
            AddressLine = req.AddressLine,
            City = req.City,
            State = req.State,
            Pincode = req.Pincode,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            IsDefault = req.IsDefault,
            AddressType = req.AddressType,
            CreatedAt = DateTime.UtcNow
        };

        _db.Add(address);
        await _db.SaveChangesAsync();

        return new AddressDto
        {
            AddressId = address.AddressId,
            AddressLine = address.AddressLine,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            IsDefault = (bool)address.IsDefault,
            AddressType = address.AddressType
        };
    }

    public async Task<bool> DeleteAsync(int addressId, int userId)
    {
        var addr = await _db.Addresses
            .FirstOrDefaultAsync(x => x.AddressId == addressId && x.UserId == userId);

        if (addr == null) return false;

        _db.Addresses.Remove(addr);
        await _db.SaveChangesAsync();

        return true;
    }
}