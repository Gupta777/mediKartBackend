using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class StockHistory
{
    public int StockHistoryId { get; set; }

    public int MedicineId { get; set; }

    public int? PreviousStock { get; set; }

    public int? ChangedStock { get; set; }

    public string? Reason { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;
}
