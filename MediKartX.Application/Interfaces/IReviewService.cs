using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface IReviewService
{
    Task<(bool ok, string? error)> AddReviewAsync(int userId, AddReviewRequest req);

    Task<List<ReviewDto>> GetReviewsAsync(int medicineId);
}