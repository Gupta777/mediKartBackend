using System;
using System.Collections.Generic;

namespace MediKartX.Infrastructure.Data;

public partial class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool? IsDefault { get; set; }

    public string? AddressType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
