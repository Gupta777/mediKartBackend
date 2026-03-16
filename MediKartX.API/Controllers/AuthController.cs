using System;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("request-otp")]
    public async Task<ActionResult<ApiResponse<object>>> RequestOtp([FromBody] RequestOtpRequest req)
    {
        var result = await _authService.RequestOtpAsync(req);
        if (!result.Success)
        {
            var resp = new ApiResponse<object>
            {
                Success = false,
                Message = "Request OTP failed",
                Errors = new[] { result.Message ?? string.Empty },
                Data = null,
                StatusCode = 400
            };
            return BadRequest(resp);
        }

        var data = new { otp = result.Otp };
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = data, Errors = null, StatusCode = 200 });
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var result = await _authService.VerifyOtpAsync(req);
        if (!result.Success)
        {
            var resp = new ApiResponse<object>
            {
                Success = false,
                Message = "Verify OTP failed",
                Errors = new[] { result.Message ?? string.Empty },
                Data = null,
                StatusCode = 400
            };
            return BadRequest(resp);
        }

        var data = new { token = result.Token, expiresInMinutes = result.ExpiresInMinutes, roles = result.Roles };
        return Ok(new ApiResponse<object> { Success = true, Message = "Authentication successful", Data = data, Errors = null, StatusCode = 200 });
    }

    [HttpPost("verify-otp-admin")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyOtpAdmin([FromBody] VerifyOtpRequest req)
    {
        var result = await _authService.VerifyOtpAsync(req);
        if (!result.Success)
        {
            var resp = new ApiResponse<object>
            {
                Success = false,
                Message = "Verify OTP failed",
                Errors = new[] { result.Message ?? string.Empty },
                Data = null,
                StatusCode = 400
            };
            return BadRequest(resp);
        }

        var roles = result.Roles ?? Array.Empty<string>();
        if (!roles.Contains("Admin"))
        {
            var resp = new ApiResponse<object>
            {
                Success = false,
                Message = "Forbidden",
                Errors = new[] { "User is not an admin" },
                Data = null,
                StatusCode = 403
            };
            return StatusCode(403, resp);
        }

        var data = new { token = result.Token, expiresInMinutes = result.ExpiresInMinutes, roles = result.Roles };
        return Ok(new ApiResponse<object> { Success = true, Message = "Admin authentication successful", Data = data, Errors = null, StatusCode = 200 });
    }
}

