using System;
namespace MediKartX.Infrastructure.Data;

public partial class Shop
{
    public int ShopId { get; set; }
    public string? ShopName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
