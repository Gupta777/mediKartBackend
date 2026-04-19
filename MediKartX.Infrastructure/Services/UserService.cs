using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly MediKartXDbContext _db;
    private readonly IMemoryCache _cache;

    public UserService(MediKartXDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // ================= UPDATE PROFILE =================
    public async Task<ApiResponse<object>> UpdateProfileAsync(int userId, UserProfileDto dto)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "User not found",
                StatusCode = 404
            };
        }

        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);

        if (address == null)
        {
            address = new Address
            {
                UserId = userId,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Addresses.Add(address);
        }

        address.AddressLine = dto.AddressLine;
        address.City = dto.City;
        address.State = dto.State;
        address.Pincode = dto.Pincode;
        address.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // ✅ Clear cache
        _cache.Remove($"user_{userId}");
        _cache.Remove($"profile_{userId}");

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Profile updated successfully",
            StatusCode = 200
        };
    }

    // ================= GET USER BY ID =================
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
    {
        var cacheKey = $"user_{userId}";

        if (!_cache.TryGetValue(cacheKey, out UserDto? cached))
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            cached = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                IsActive = user.IsActive ?? false
            };

            _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(10));
        }

        return new ApiResponse<UserDto>
        {
            Success = true,
            Message = "User retrieved",
            Data = cached,
            StatusCode = 200
        };
    }

    // ================= GET PROFILE =================
    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var cacheKey = $"profile_{userId}";

        if (_cache.TryGetValue(cacheKey, out UserProfileDto cachedProfile))
        {
            return cachedProfile;
        }

        var user = await _db.Users
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.UserId,
                u.MobileNumber,
                u.Email,
                u.IsMobileVerified
            })
            .FirstOrDefaultAsync();

        if (user == null) return null;

        var role = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.RoleName)
            .FirstOrDefaultAsync();

        var address = await _db.Addresses
            .Where(a => a.UserId == userId && a.IsDefault == true)
            .FirstOrDefaultAsync();

        var profile = new UserProfileDto
        {
            UserId = user.UserId,
            MobileNumber = user.MobileNumber,
            Email = user.Email,
            IsMobileVerified = user.IsMobileVerified,
            Role = role,

            AddressLine = address?.AddressLine,
            City = address?.City,
            State = address?.State,
            Pincode = address?.Pincode
        };

        // ✅ Cache profile
        _cache.Set(cacheKey, profile, TimeSpan.FromMinutes(5));

        return profile;
    }
}