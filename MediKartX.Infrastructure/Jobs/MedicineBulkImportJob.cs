using System.IO;
using System.Threading.Tasks;
using Hangfire;
using MediKartX.Application.Interfaces;

namespace MediKartX.Infrastructure.Jobs;

public class MedicineBulkImportJob
{
    private readonly IMedicineService _medicineService;

    public MedicineBulkImportJob(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    // filePath is a local path on the server where the uploaded file was saved
    public async Task ProcessFileAsync(string filePath)
    {
        await using var fs = File.OpenRead(filePath);
        await _medicineService.BulkUploadFileAsync(fs, Path.GetFileName(filePath), useTransaction: true);
        // optional: delete file after processing
        try { File.Delete(filePath); } catch { }
    }
}
