namespace MediKartX.Application.DTOs;

public class RequestOtpRequest
{
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
}

public class VerifyOtpRequest
{
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Otp { get; set; } // dev only
    public string? Token { get; set; }
    public int? ExpiresInMinutes { get; set; }
    public string[]? Roles { get; set; }
}
