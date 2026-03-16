using System;
using System.ComponentModel.DataAnnotations;
namespace MediKartX.Application.DTOs;

public class MedicineDto
{
    public int MedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string? BrandName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? PackSize { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    public decimal Mrp { get; set; }
    public decimal SellingPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int? Gstpercent { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMedicineDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int BrandId { get; set; }

    [Required]
    public int CategoryId { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? PackSize { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    [Range(0.01, 100000)]
    public decimal Mrp { get; set; }

    [Range(0.01, 100000)]
    public decimal SellingPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int? Gstpercent { get; set; }
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateMedicineDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? Name { get; set; }

    public int? BrandId { get; set; }

    public int? CategoryId { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? PackSize { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    [Range(0.01, 100000)]
    public decimal? Mrp { get; set; }

    [Range(0.01, 100000)]
    public decimal? SellingPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int? Gstpercent { get; set; }
    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public T[] Items { get; set; } = Array.Empty<T>();
}
