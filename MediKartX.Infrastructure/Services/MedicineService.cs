using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class MedicineService : IMedicineService
{
    private readonly MediKartXDbContext _db;

    public MedicineService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<MedicineDto> AddMedicineAsync(CreateMedicineDto dto)
    {
        var entity = new Medicine
        {
            Name = dto.Name,
            BrandId = dto.BrandId,
            CategoryId = dto.CategoryId,
            Strength = dto.Strength,
            DosageForm = dto.DosageForm,
            PackSize = dto.PackSize,
            IsPrescriptionRequired = dto.IsPrescriptionRequired,
            Mrp = dto.Mrp,
            SellingPrice = dto.SellingPrice,
            DiscountPercent = dto.DiscountPercent,
            Gstpercent = dto.Gstpercent,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Medicines.Add(entity);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(entity.MedicineId) ?? throw new Exception("Failed to load created medicine");
    }

    public async Task<MedicineDto?> UpdateMedicineAsync(int id, UpdateMedicineDto dto)
    {
        var entity = await _db.Medicines.FindAsync(id);
        if (entity == null) return null;
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.BrandId.HasValue) entity.BrandId = dto.BrandId.Value;
        if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId.Value;
        if (dto.Strength != null) entity.Strength = dto.Strength;
        if (dto.DosageForm != null) entity.DosageForm = dto.DosageForm;
        if (dto.PackSize != null) entity.PackSize = dto.PackSize;
        if (dto.IsPrescriptionRequired.HasValue) entity.IsPrescriptionRequired = dto.IsPrescriptionRequired;
        if (dto.Mrp.HasValue) entity.Mrp = dto.Mrp.Value;
        if (dto.SellingPrice.HasValue) entity.SellingPrice = dto.SellingPrice.Value;
        if (dto.DiscountPercent.HasValue) entity.DiscountPercent = dto.DiscountPercent;
        if (dto.Gstpercent.HasValue) entity.Gstpercent = dto.Gstpercent;
        if (dto.Stock.HasValue) entity.Stock = dto.Stock.Value;
        if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(entity.MedicineId);
    }

    public async Task<bool> DeleteMedicineAsync(int id)
    {
        var entity = await _db.Medicines.FindAsync(id);
        if (entity == null) return false;
        // soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var q = await _db.Medicines
            .Include(m => m.Brand)
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.MedicineId == id);
        if (q == null) return null;
        return MapToDto(q);
    }

    public async Task<PagedResult<MedicineDto>> GetAllAsync(int page, int pageSize, string? search, int? categoryId)
    {
        var query = _db.Medicines.AsQueryable();
        query = query.Where(m => m.IsActive == true || m.IsActive == null);
        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var items = await query
            .Include(m => m.Brand)
            .Include(m => m.Category)
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoItems = items.Select(MapToDto).ToArray();

        return new PagedResult<MedicineDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages,
            Items = dtoItems
        };
    }

    public async Task<(int created, string[] errors)> BulkUploadFromCsvAsync(System.IO.Stream csvStream)
    {
        var errors = new System.Collections.Generic.List<string>();
        int created = 0;

        using var reader = new System.IO.StreamReader(csvStream);
        string? header = await reader.ReadLineAsync();
        if (header == null) return (0, new[] { "Empty file" });

        var cols = header.Split(',').Select(c => c.Trim().ToLower()).ToArray();

        // Preload existing brands/categories and medicine keys to avoid duplicates
        var brandDict = await _db.Brands
            .AsNoTracking()
            .ToDictionaryAsync(b => b.BrandName.ToLower(), b => b.BrandId);
        var categoryDict = await _db.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.CategoryName.ToLower(), c => c.CategoryId);

        var existingMedicineKeys = new System.Collections.Generic.HashSet<string>(
            await _db.Medicines
                .AsNoTracking()
                .Select(m => ((m.Name ?? string.Empty).Trim().ToLower() + "|" + m.BrandId + "|" + m.CategoryId))
                .Distinct()
                .ToListAsync()
        );

        // Helper to map header name to value
        string Get(string name, string[] parts)
        {
            var idx = Array.IndexOf(cols, name);
            return idx >= 0 && idx < parts.Length ? parts[idx].Trim() : string.Empty;
        }

        int lineNo = 1;
        while (!reader.EndOfStream)
        {
            lineNo++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(',');
            try
            {
                var name = Get("name", parts);
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Line {lineNo}: medicine name is required");
                    continue;
                }

                // Resolve brand id: prefer explicit brandid, otherwise brandname (create if missing)
                int brandId = 0;
                var brandIdStr = Get("brandid", parts);
                var brandName = Get("brandname", parts);
                if (!string.IsNullOrWhiteSpace(brandIdStr) && int.TryParse(brandIdStr, out var parsedBid) && parsedBid > 0)
                {
                    var exists = await _db.Brands.AnyAsync(b => b.BrandId == parsedBid);
                    if (!exists)
                    {
                        errors.Add($"Line {lineNo}: brand id {parsedBid} not found");
                        continue;
                    }
                    brandId = parsedBid;
                }
                else if (!string.IsNullOrWhiteSpace(brandName))
                {
                    var key = brandName.Trim().ToLower();
                    if (!brandDict.TryGetValue(key, out brandId))
                    {
                        // create new brand
                        var nb = new Brand { BrandName = brandName.Trim() };
                        _db.Brands.Add(nb);
                        await _db.SaveChangesAsync();
                        brandId = nb.BrandId;
                        brandDict[key] = brandId;
                    }
                }
                else
                {
                    errors.Add($"Line {lineNo}: brand id or brand name required");
                    continue;
                }

                // Resolve category id: prefer explicit categoryid, otherwise categoryname (create if missing)
                int categoryId = 0;
                var categoryIdStr = Get("categoryid", parts);
                var categoryName = Get("categoryname", parts);
                if (!string.IsNullOrWhiteSpace(categoryIdStr) && int.TryParse(categoryIdStr, out var parsedCid) && parsedCid > 0)
                {
                    var exists = await _db.Categories.AnyAsync(c => c.CategoryId == parsedCid);
                    if (!exists)
                    {
                        errors.Add($"Line {lineNo}: category id {parsedCid} not found");
                        continue;
                    }
                    categoryId = parsedCid;
                }
                else if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var key = categoryName.Trim().ToLower();
                    if (!categoryDict.TryGetValue(key, out categoryId))
                    {
                        var nc = new Category { CategoryName = categoryName.Trim() };
                        _db.Categories.Add(nc);
                        await _db.SaveChangesAsync();
                        categoryId = nc.CategoryId;
                        categoryDict[key] = categoryId;
                    }
                }
                else
                {
                    errors.Add($"Line {lineNo}: category id or category name required");
                    continue;
                }

                var keyCheck = (name ?? string.Empty).Trim().ToLower() + "|" + brandId + "|" + categoryId;
                if (existingMedicineKeys.Contains(keyCheck))
                {
                    errors.Add($"Line {lineNo}: duplicate medicine (name+brand+category) - skipped");
                    continue;
                }

                var dto = new CreateMedicineDto
                {
                    Name = name,
                    BrandId = brandId,
                    CategoryId = categoryId,
                    Mrp = decimal.TryParse(Get("mrp", parts), out var mrpVal) ? mrpVal : 0,
                    SellingPrice = decimal.TryParse(Get("sellingprice", parts), out var spVal) ? spVal : 0,
                    Stock = int.TryParse(Get("stock", parts), out var stockVal) ? stockVal : 0,
                    Strength = Get("strength", parts),
                    DosageForm = Get("dosageform", parts),
                    PackSize = Get("packsize", parts),
                    IsPrescriptionRequired = (Get("isprescriptionrequired", parts)).ToLower() == "true",
                    ImageUrl = Get("imageurl", parts)
                };

                // basic validation
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
                var valResults = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, context, valResults, true))
                {
                    errors.Add($"Line {lineNo}: " + string.Join("; ", valResults.Select(v => v.ErrorMessage)));
                    continue;
                }

                var entity = new Medicine
                {
                    Name = dto.Name,
                    BrandId = dto.BrandId,
                    CategoryId = dto.CategoryId,
                    Strength = dto.Strength,
                    DosageForm = dto.DosageForm,
                    PackSize = dto.PackSize,
                    IsPrescriptionRequired = dto.IsPrescriptionRequired,
                    Mrp = dto.Mrp,
                    SellingPrice = dto.SellingPrice,
                    DiscountPercent = dto.DiscountPercent,
                    Gstpercent = dto.Gstpercent,
                    Stock = dto.Stock,
                    ImageUrl = dto.ImageUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Medicines.Add(entity);
                existingMedicineKeys.Add(keyCheck);
                created++;
            }
            catch (Exception ex)
            {
                errors.Add($"Line {lineNo}: {ex.Message}");
            }
        }

        // persist created medicines (brands/categories were saved when created)
        await _db.SaveChangesAsync();
        return (created, errors.ToArray());
    }

    public async Task<(int created, string[] errors)> BulkUploadFileAsync(System.IO.Stream fileStream, string fileName, bool useTransaction = true)
    {
        // if excel, convert to csv-like stream using EPPlus, otherwise pass through
        var lower = (fileName ?? string.Empty).ToLower();
        if (lower.EndsWith(".xlsx") || lower.EndsWith(".xls"))
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new OfficeOpenXml.ExcelPackage(fileStream);
            var sb = new System.Text.StringBuilder();
            foreach (var sheet in package.Workbook.Worksheets)
            {
                var dim = sheet.Dimension;
                if (dim == null) continue;
                var start = dim.Start;
                var end = dim.End;
                // header
                for (int c = start.Column; c <= end.Column; c++)
                {
                    if (c > start.Column) sb.Append(',');
                    sb.Append(sheet.Cells[start.Row, c].Text);
                }
                sb.AppendLine();
                for (int r = start.Row + 1; r <= end.Row; r++)
                {
                    for (int c = start.Column; c <= end.Column; c++)
                    {
                        if (c > start.Column) sb.Append(',');
                        var text = sheet.Cells[r, c].Text;
                        if (text != null && (text.Contains(',') || text.Contains('\n') || text.Contains('"')))
                        {
                            // crude quoting
                            sb.Append('"').Append(text.Replace("\"", "\"\"")).Append('"');
                        }
                        else sb.Append(text);
                    }
                    sb.AppendLine();
                }
                break; // only first sheet
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            await using var ms = new System.IO.MemoryStream(bytes);
            if (useTransaction)
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                var res = await BulkUploadFromCsvAsync(ms);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return res;
            }
            else
            {
                return await BulkUploadFromCsvAsync(ms);
            }
        }
        else
        {
            if (useTransaction)
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                var res = await BulkUploadFromCsvAsync(fileStream);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return res;
            }
            else
            {
                return await BulkUploadFromCsvAsync(fileStream);
            }
        }
    }

    private MedicineDto MapToDto(Medicine m)
    {
        return new MedicineDto
        {
            MedicineId = m.MedicineId,
            Name = m.Name,
            BrandId = m.BrandId,
            BrandName = m.Brand?.BrandName,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.CategoryName,
            Strength = m.Strength,
            DosageForm = m.DosageForm,
            PackSize = m.PackSize,
            IsPrescriptionRequired = m.IsPrescriptionRequired,
            Mrp = m.Mrp,
            SellingPrice = m.SellingPrice,
            DiscountPercent = m.DiscountPercent,
            Gstpercent = m.Gstpercent,
            Stock = m.Stock,
            ImageUrl = m.ImageUrl,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
    }
}
