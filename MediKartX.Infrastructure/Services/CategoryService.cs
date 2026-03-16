using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;
using System;

namespace MediKartX.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly MediKartXDbContext _db;

    public CategoryService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _db.Categories.AsNoTracking().Where(c => c.IsActive == true || c.IsActive == null);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CategoryName.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.CategoryName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryType = c.CategoryType,
                IsActive = c.IsActive ?? true,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToListAsync();

        return new PagedResult<CategoryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Items = items.ToArray()
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return null;
        return new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryType = c.CategoryType,
            IsActive = c.IsActive ?? true,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        // unique name check
        var exists = await _db.Categories.AnyAsync(c => c.CategoryName == dto.CategoryName);
        if (exists) throw new Exception("Category name already exists");
        var entity = new Category { CategoryName = dto.CategoryName, CategoryType = dto.CategoryType, IsActive = true, CreatedAt = DateTime.UtcNow };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return new CategoryDto { CategoryId = entity.CategoryId, CategoryName = entity.CategoryName, CategoryType = entity.CategoryType, IsActive = entity.IsActive ?? true, CreatedAt = entity.CreatedAt };
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity == null) return null;
        if (!string.IsNullOrWhiteSpace(dto.CategoryName) && dto.CategoryName != entity.CategoryName)
        {
            var exists = await _db.Categories.AnyAsync(c => c.CategoryName == dto.CategoryName && c.CategoryId != id);
            if (exists) throw new Exception("Category name already exists");
            entity.CategoryName = dto.CategoryName;
        }
        if (dto.CategoryType != null) entity.CategoryType = dto.CategoryType;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity == null) return false;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
