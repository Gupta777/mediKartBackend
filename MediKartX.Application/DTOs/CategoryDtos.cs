using System;
namespace MediKartX.Application.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryType { get; set; }
}

public class UpdateCategoryDto
{
    public string? CategoryName { get; set; }
    public string? CategoryType { get; set; }
    public bool? IsActive { get; set; }
}
