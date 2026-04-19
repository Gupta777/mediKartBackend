using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediKartX.Application.DTOs;
using MediKartX.Application.Interfaces;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;


public class ReviewService : IReviewService
{
    private readonly MediKartXDbContext _db;

    public ReviewService(MediKartXDbContext db)
    {
        _db = db;
    }

    public async Task<(bool ok, string? error)> AddReviewAsync(int userId, AddReviewRequest req)
    {
        if (req.Rating < 1 || req.Rating > 5)
            return (false, "Rating must be between 1 and 5");

        var exists = await _db.ProductReviews
            .AnyAsync(r => r.UserId == userId && r.MedicineId == req.MedicineId);

        if (exists)
            return (false, "You already reviewed this product");

        _db.ProductReviews.Add(new ProductReview
        {
            UserId = userId,
            MedicineId = req.MedicineId,
            Rating = req.Rating,
            Comment = req.Comment,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<List<ReviewDto>> GetReviewsAsync(int medicineId)
    {
        return await _db.ProductReviews
            .Where(r => r.MedicineId == medicineId)
            .Include(r => r.User)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                MedicineId = r.MedicineId,
                UserName = r.User.MobileNumber, // or name if added later
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToListAsync();
    }
}