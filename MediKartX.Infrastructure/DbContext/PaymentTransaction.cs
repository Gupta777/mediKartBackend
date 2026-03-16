using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class PaymentTransaction
{
    public int TransactionId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public string? TransactionReference { get; set; }

    public decimal Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
