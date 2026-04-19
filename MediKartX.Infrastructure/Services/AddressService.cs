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

    public async Task<List<AddressDto>> GetUserAddressesAsync(int userId)
    {
        return await _db.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressLine = a.AddressLine,
                City = a.City,
                State = a.State,
                Pincode = a.Pincode,
                IsDefault = a.IsDefault,
                AddressType = a.AddressType
            })
            .ToListAsync();
    }

    public async Task<AddressDto?> AddAddressAsync(int userId, AddAddressRequest req)
    {
        if (req.IsDefault)
        {
            var existing = await _db.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

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
            IsDefault = req.IsDefault,
            AddressType = req.AddressType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return new AddressDto
        {
            AddressId = address.AddressId,
            AddressLine = address.AddressLine,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            IsDefault = address.IsDefault,
            AddressType = address.AddressType
        };
    }

    public async Task<bool> UpdateAddressAsync(int userId, UpdateAddressRequest req)
    {
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.AddressId == req.AddressId && a.UserId == userId);

        if (address == null) return false;

        if (req.IsDefault)
        {
            var all = await _db.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            foreach (var addr in all)
                addr.IsDefault = false;
        }

        address.AddressLine = req.AddressLine;
        address.City = req.City;
        address.State = req.State;
        address.Pincode = req.Pincode;
        address.IsDefault = req.IsDefault;
        address.AddressType = req.AddressType;
        address.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAddressAsync(int userId, int addressId)
    {
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

        if (address == null) return false;

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
    {
        var addresses = await _db.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();

        foreach (var addr in addresses)
            addr.IsDefault = addr.AddressId == addressId;

        await _db.SaveChangesAsync();
        return true;
    }
}
