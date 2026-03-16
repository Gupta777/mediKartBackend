using System;

namespace MediKartX.Infrastructure.Data;

public partial class OrderHistory
{
    public int OrderHistoryId { get; set; }
    public int OrderId { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? Note { get; set; }

    public virtual Order Order { get; set; } = null!;
}
