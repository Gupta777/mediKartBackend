using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _service;

    public AddressController(IAddressService service)
    {
        _service = service;
    }

    private int GetUserId()
    {
        return int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        );
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var data = await _service.GetUserAddressesAsync(userId);

        return Ok(new ApiResponse<List<AddressDto>>
        {
            Success = true,
            Message = "Addresses retrieved",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddAddressRequest req)
    {
        var userId = GetUserId();
        var data = await _service.AddAddressAsync(userId, req);

        return Ok(new ApiResponse<AddressDto>
        {
            Success = true,
            Message = "Address added successfully",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateAddressRequest req)
    {
        var userId = GetUserId();
        var ok = await _service.UpdateAddressAsync(userId, req);

        if (!ok)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Address not found",
                Errors = new[] { "Invalid addressId" },
                StatusCode = 404
            });

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Address updated successfully",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpDelete("{addressId}")]
    public async Task<IActionResult> Delete(int addressId)
    {
        var userId = GetUserId();
        var ok = await _service.DeleteAddressAsync(userId, addressId);

        if (!ok)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Address not found",
                Errors = new[] { "Invalid addressId" },
                StatusCode = 404
            });

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Address deleted successfully",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("set-default/{addressId}")]
    public async Task<IActionResult> SetDefault(int addressId)
    {
        var userId = GetUserId();
        await _service.SetDefaultAddressAsync(userId, addressId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Default address updated",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }
}
