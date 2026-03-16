using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MediKartX.Infrastructure.Data;
using MediKartX.Infrastructure.Services;

namespace MediKartX.Tests;

public class MedicineServiceTests
{
    private MediKartXDbContext CreateDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<MediKartXDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new MediKartXDbContext(options);
    }

    [Fact]
    public async Task BulkUpload_FromCsv_CreatesRecords()
    {
        var db = CreateDbContext("meds1");
        // seed required Brand and Category
        db.Brands.Add(new Brand { BrandName = "B1" });
        db.Categories.Add(new Category { CategoryName = "C1" });
        await db.SaveChangesAsync();

        var svc = new MedicineService(db);
        var csv = new StringBuilder();
        csv.AppendLine("name,brandid,categoryid,mrp,sellingprice,stock,strength,dosageform,packsize,isprescriptionrequired,imageurl");
        csv.AppendLine("Paracetamol,1,1,50,45,100,500mg,Tablet,10,False,https://example.com/img.jpg");

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
        var (created, errors) = await svc.BulkUploadFromCsvAsync(ms);
        Assert.Equal(1, created);
        Assert.Empty(errors);
    }
}
