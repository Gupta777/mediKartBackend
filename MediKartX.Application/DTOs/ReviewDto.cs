public class AddReviewRequest
{
    public int MedicineId { get; set; }
    public int Rating { get; set; } // 1–5
    public string? Comment { get; set; }
}

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int MedicineId { get; set; }
    public string? UserName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
}