using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class WishlistItem
{
    public int WishlistItemId { get; set; }

    public int WishlistId { get; set; }

    public int MedicineId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual Wishlist Wishlist { get; set; } = null!;
}
