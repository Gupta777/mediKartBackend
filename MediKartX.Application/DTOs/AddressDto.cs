using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class AddressDto
{
    public int AddressId { get; set; }
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Pincode { get; set; }
    public bool? IsDefault { get; set; }
    public string AddressType { get; set; }
}

public class AddAddressRequest
{
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Pincode { get; set; }
    public bool IsDefault { get; set; }
    public string AddressType { get; set; } // Shipping / Billing
}

public class UpdateAddressRequest : AddAddressRequest
{
    public int AddressId { get; set; }
}