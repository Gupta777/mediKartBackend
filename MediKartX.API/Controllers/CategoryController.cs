using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _svc;

    public CategoryController(ICategoryService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var res = await _svc.GetAllAsync(page, pageSize, search);
        return Ok(new ApiResponse<PagedResult<CategoryDto>> { Success = true, Message = "Categories retrieved", Data = res, StatusCode = 200 });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var res = await _svc.GetByIdAsync(id);
        if (res == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Category not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<CategoryDto> { Success = true, Message = "Category retrieved", Data = res, StatusCode = 200 });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var res = await _svc.CreateAsync(dto);
            return Ok(new ApiResponse<CategoryDto> { Success = true, Message = "Category created", Data = res, StatusCode = 200 });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Create failed", Errors = new[] { ex.Message }, StatusCode = 400 });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var res = await _svc.UpdateAsync(id, dto);
            if (res == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Category not found" }, StatusCode = 404 });
            return Ok(new ApiResponse<CategoryDto> { Success = true, Message = "Category updated", Data = res, StatusCode = 200 });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Update failed", Errors = new[] { ex.Message }, StatusCode = 400 });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _svc.SoftDeleteAsync(id);
        if (!ok) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Category not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<object> { Success = true, Message = "Category deleted", Data = null, StatusCode = 200 });
    }
}
