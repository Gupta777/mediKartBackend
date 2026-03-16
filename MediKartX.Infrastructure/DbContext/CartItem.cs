using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int MedicineId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;
}
