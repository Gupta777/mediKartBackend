using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMode { get; set; }

    public string? OrderStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<RewardTransaction> RewardTransactions { get; set; } = new List<RewardTransaction>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual User User { get; set; } = null!;
}
