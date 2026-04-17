using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AddressRequest
{
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Pincode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public string AddressType { get; set; } // Shipping / Billing
}

public class AddressDto
{
    public int AddressId { get; set; }
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Pincode { get; set; }
    public bool IsDefault { get; set; }
    public string AddressType { get; set; }
}