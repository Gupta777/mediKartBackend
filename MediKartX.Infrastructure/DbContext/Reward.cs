using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Reward
{
    public int RewardId { get; set; }

    public int UserId { get; set; }

    public int? Points { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RewardTransaction> RewardTransactions { get; set; } = new List<RewardTransaction>();

    public virtual User User { get; set; } = null!;
}
