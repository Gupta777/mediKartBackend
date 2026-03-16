using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int MedicineId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
