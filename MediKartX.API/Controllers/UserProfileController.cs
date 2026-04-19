using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public UserProfileController(IUserService userService)
    {
        _userService = userService;
    }

    // ✅ COMMON METHOD
    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null || string.IsNullOrEmpty(claim.Value))
            throw new UnauthorizedAccessException("Invalid token");

        return int.Parse(claim.Value);
    }

    // ================= UPDATE PROFILE =================
    [Authorize(Roles = "User,Admin")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request",
                    Data = null,
                    Errors = new[] { "Invalid input" },
                    StatusCode = 400,
                    Timestamp = DateTime.UtcNow
                });
            }

            int userId = GetUserId();

            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message,
                    Data = null,
                    Errors = new[] { result.Message ?? "Error" },
                    StatusCode = result.StatusCode,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = result.Message,
                Data = null,
                Errors = null,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new[] { ex.Message },
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Something went wrong",
                Data = null,
                Errors = new[] { ex.Message },
                StatusCode = 500,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    // ================= GET PROFILE =================
    [Authorize(Roles = "User,Admin")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            int userId = GetUserId();

            var profile = await _userService.GetProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Message = "User not found",
                    Data = null,
                    Errors = new[] { "User not found" },
                    StatusCode = 404,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<UserProfileDto>
            {
                Success = true,
                Message = "Profile fetched successfully",
                Data = profile,
                Errors = null,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<UserProfileDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new[] { ex.Message },
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UserProfileDto>
            {
                Success = false,
                Message = "Something went wrong",
                Data = null,
                Errors = new[] { ex.Message },
                StatusCode = 500,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}