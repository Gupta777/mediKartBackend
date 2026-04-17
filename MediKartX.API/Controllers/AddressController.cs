using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;
[ApiController]
[Route("api/address")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _svc;

    public AddressController(IAddressService svc)
    {
        _svc = svc;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _svc.GetAsync(GetUserId());

        return Ok(new ApiResponse<List<AddressDto>>
        {
            Success = true,
            Message = "Address list retrieved",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddressRequest req)
    {
        var data = await _svc.AddAsync(GetUserId(), req);

        return Ok(new ApiResponse<AddressDto>
        {
            Success = true,
            Message = "Address added successfully",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _svc.DeleteAsync(id, GetUserId());

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
            Message = "Address deleted",
            StatusCode = 200
        });
    }
}