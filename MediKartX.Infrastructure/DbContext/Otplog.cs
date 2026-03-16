using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Otplog
{
    public int Otpid { get; set; }

    public int? UserId { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string Otpcode { get; set; } = null!;

    public bool? IsUsed { get; set; }

    public DateTime ExpiryTime { get; set; }

    public DateTime? AttemptedAt { get; set; }

    public int? AttemptCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
