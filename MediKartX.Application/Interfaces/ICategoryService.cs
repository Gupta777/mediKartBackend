using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> SoftDeleteAsync(int id);
}
