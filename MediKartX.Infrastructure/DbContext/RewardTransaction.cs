using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class RewardTransaction
{
    public int RewardTransactionId { get; set; }

    public int RewardId { get; set; }

    public int? OrderId { get; set; }

    public int PointsChanged { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Reward Reward { get; set; } = null!;
}
