using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class ProductReview
{
    public int ReviewId { get; set; }

    public int MedicineId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
