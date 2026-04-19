using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _svc;

    public ReviewController(IReviewService svc)
    {
        _svc = svc;
    }

    // ⭐ Add Review (Login required)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add(AddReviewRequest req)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

        var result = await _svc.AddReviewAsync(userId, req);

        if (!result.ok)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.error,
                Errors = new[] { result.error },
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Review added successfully",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }

    // ⭐ Get Reviews (Public)
    [HttpGet("{medicineId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int medicineId)
    {
        var data = await _svc.GetReviewsAsync(medicineId);

        return Ok(new ApiResponse<List<ReviewDto>>
        {
            Success = true,
            Message = "Reviews retrieved",
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        });
    }
}