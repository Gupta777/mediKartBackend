using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface IMedicineService
{
    Task<MedicineDto> AddMedicineAsync(CreateMedicineDto dto);
    Task<MedicineDto?> UpdateMedicineAsync(int id, UpdateMedicineDto dto);
    Task<bool> DeleteMedicineAsync(int id);
    Task<MedicineDto?> GetByIdAsync(int id);
    Task<PagedResult<MedicineDto>> GetAllAsync(int page, int pageSize, string? search, int? categoryId);
    Task<(int created, string[] errors)> BulkUploadFromCsvAsync(System.IO.Stream csvStream);

    Task<(int created, string[] errors)> BulkUploadFileAsync(System.IO.Stream fileStream, string fileName, bool useTransaction = true);
}
