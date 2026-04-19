using System.Collections.Generic;

namespace MediKartX.Application.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UserProfileDto
{
    public int UserId { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public bool? IsMobileVerified { get; set; }

    public string? Role { get; set; }

    // Address
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
}

