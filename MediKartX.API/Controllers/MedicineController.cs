using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediKartX.Application.Interfaces;
using Hangfire;
using MediKartX.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MediKartX.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicineController : ControllerBase
{
    private readonly IMedicineService _svc;

    public MedicineController(IMedicineService svc)
    {
        _svc = svc;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MedicineDto>>> Add([FromBody] CreateMedicineDto dto)
    {
        var created = await _svc.AddMedicineAsync(dto);
        return Ok(new ApiResponse<MedicineDto> { Success = true, Message = "Medicine created", Data = created, StatusCode = 200 });
    }

    /// <summary>
    /// Bulk upload medicines via CSV. CSV header should include: name,brandid,categoryid,mrp,sellingprice,stock,strength,dosageform,packsize,isprescriptionrequired,imageurl
    /// Admin only.
    /// </summary>
    [HttpPost("bulk-upload")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> BulkUpload(IFormFile file, [FromQuery] bool background = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "No file uploaded", Errors = new[] { "No file provided" }, StatusCode = 400 });
        var fname = file.FileName ?? "upload";
        var ext = System.IO.Path.GetExtension(fname).ToLower();
        if (!(ext == ".csv" || ext == ".xlsx" || ext == ".xls"))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid file type", Errors = new[] { "Only CSV or Excel files are supported" }, StatusCode = 400 });

        if (background)
        {
            // save to temp and enqueue job
            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ext);
            await using (var fs = System.IO.File.Create(tmp))
            {
                await file.CopyToAsync(fs);
            }

            // enqueue background job
            var bg = HttpContext.RequestServices.GetService(typeof(Hangfire.IBackgroundJobClient)) as Hangfire.IBackgroundJobClient;
            if (bg == null) return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Background job client not available", Errors = new[] { "Hangfire not configured" }, StatusCode = 500 });
            bg.Enqueue<MediKartX.Infrastructure.Jobs.MedicineBulkImportJob>(j => j.ProcessFileAsync(tmp));
            return Ok(new ApiResponse<object> { Success = true, Message = "Bulk upload enqueued", Data = new { path = tmp }, StatusCode = 200 });
        }

        using var stream = file.OpenReadStream();
        var (created, errors) = await _svc.BulkUploadFileAsync(stream, fname, useTransaction: true);
        var data = new { created, errors };
        return Ok(new ApiResponse<object> { Success = true, Message = "Bulk upload completed", Data = data, StatusCode = 200 });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MedicineDto?>>> Update(int id, [FromBody] UpdateMedicineDto dto)
    {
        var updated = await _svc.UpdateMedicineAsync(id, dto);
        if (updated == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Medicine not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<MedicineDto> { Success = true, Message = "Medicine updated", Data = updated, StatusCode = 200 });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _svc.DeleteMedicineAsync(id);
        if (!ok) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Medicine not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<object> { Success = true, Message = "Medicine deleted", Data = null, StatusCode = 200 });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicineDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? categoryId = null)
    {
        var res = await _svc.GetAllAsync(page, pageSize, search, categoryId);
        return Ok(new ApiResponse<PagedResult<MedicineDto>> { Success = true, Message = "Medicines retrieved", Data = res, StatusCode = 200 });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MedicineDto>>> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Not found", Errors = new[] { "Medicine not found" }, StatusCode = 404 });
        return Ok(new ApiResponse<MedicineDto> { Success = true, Message = "Medicine retrieved", Data = dto, StatusCode = 200 });
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicineDto>>>> FilterByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var res = await _svc.GetAllAsync(page, pageSize, null, categoryId);
        return Ok(new ApiResponse<PagedResult<MedicineDto>> { Success = true, Message = "Medicines retrieved", Data = res, StatusCode = 200 });
    }
}
