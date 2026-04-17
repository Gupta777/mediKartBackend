using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

class UserService : IUserService
{
    private readonly MediKartXDbContext _db;
    private readonly IMemoryCache _cache;

    public UserService(MediKartXDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
    {
        var cacheKey = $"user_{userId}";
        if (!_cache.TryGetValue(cacheKey, out UserDto? cached))
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return new ApiResponse<UserDto> { Success = false, Message = "User not found", StatusCode = 404 };

            var userDto = new UserDto { UserId = user.UserId, Email = user.Email, IsActive = (bool)user.IsActive };
            _cache.Set(cacheKey, userDto, TimeSpan.FromMinutes(10));
            cached = userDto;
        }

        return new ApiResponse<UserDto> { Success = true, Message = "User retrieved", Data = cached, StatusCode = 200 };
    }
}