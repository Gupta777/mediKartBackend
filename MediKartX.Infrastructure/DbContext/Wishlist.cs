using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Wishlist
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
