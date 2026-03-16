using System;

namespace MediKartX.Infrastructure.Data;

public partial class OrderDispatch
{
    public int OrderDispatchId { get; set; }
    public int OrderId { get; set; }
    // JSON serialized shop id array in preferred order
    public string? ShopIdsJson { get; set; }
    public int CurrentIndex { get; set; }
    public DateTime? StartedAt { get; set; }
    public bool? IsCompleted { get; set; }
}
