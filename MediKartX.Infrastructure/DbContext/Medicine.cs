using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public string Name { get; set; } = null!;

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    public string? Strength { get; set; }

    public string? DosageForm { get; set; }

    public string? PackSize { get; set; }

    public bool? IsPrescriptionRequired { get; set; }

    public decimal Mrp { get; set; }

    public decimal SellingPrice { get; set; }

    public int? DiscountPercent { get; set; }

    public int? Gstpercent { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<StockHistory> StockHistories { get; set; } = new List<StockHistory>();

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
