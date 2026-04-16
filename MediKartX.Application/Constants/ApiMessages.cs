using MediKartX.Application.Enums;

namespace MediKartX.Application.Constants;

public static class ApiMessages
{
    public static string Get(ApiMessageKey key) => key switch
    {
        ApiMessageKey.RequestOtpFailed => "Request OTP failed",
        ApiMessageKey.VerifyOtpFailed => "Verify OTP failed",
        ApiMessageKey.AuthenticationSuccessful => "Authentication successful",
        ApiMessageKey.AdminAuthenticationSuccessful => "Admin authentication successful",
        ApiMessageKey.Forbidden => "Forbidden",
        ApiMessageKey.UserNotAdmin => "User is not an admin",
        _ => key.ToString()
    };
}
