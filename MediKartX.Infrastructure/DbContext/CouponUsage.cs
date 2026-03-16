using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class CouponUsage
{
    public int CouponUsageId { get; set; }

    public int CouponId { get; set; }

    public int UserId { get; set; }

    public int OrderId { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
