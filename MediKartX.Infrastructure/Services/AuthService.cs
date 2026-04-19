using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MediKartX.Application.Interfaces;
using MediKartX.Application.DTOs;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MediKartXDbContext _db;
    private readonly IConfiguration _cfg;

    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;

    public AuthService(MediKartXDbContext db, IConfiguration cfg, ISmsSender smsSender, IEmailSender emailSender)
    {
        _db = db;
        _cfg = cfg;
        _smsSender = smsSender;
        _emailSender = emailSender;
    }

    public async Task<AuthResultDto> RequestOtpAsync(RequestOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MobileNumber) && string.IsNullOrWhiteSpace(request.Email))
            return new AuthResultDto { Success = false, Message = "Provide mobileNumber or email" };

        // Normalize and validate mobile if provided
        string? normalizedMobile = null;
        if (!string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            // strip non-digits
            var digits = Regex.Replace(request.MobileNumber!, "\\D", "");
            if (digits.Length == 12 && digits.StartsWith("91")) digits = digits.Substring(2);
            if (digits.Length == 11 && digits.StartsWith("0")) digits = digits.Substring(1);
            // validate Indian 10-digit starting with 6-9
            if (!Regex.IsMatch(digits, "^[6-9]\\d{9}$"))
                return new AuthResultDto { Success = false, Message = "Invalid Indian mobile number" };
            normalizedMobile = digits;
        }

        // Validate email if provided
        if (string.IsNullOrWhiteSpace(normalizedMobile) && !string.IsNullOrWhiteSpace(request.Email))
        {
            try
            {
                var _ = new MailAddress(request.Email!);
            }
            catch
            {
                return new AuthResultDto { Success = false, Message = "Invalid email address" };
            }
        }

        var user = !string.IsNullOrWhiteSpace(normalizedMobile)
            ? _db.Users.FirstOrDefault(u => u.MobileNumber == normalizedMobile)
            : _db.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
        {
            user = new User
            {
                MobileNumber = normalizedMobile ?? string.Empty,
                Email = request.Email,
                IsActive = true,
                IsMobileVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Ensure roles exist and assign default role
            var userRoleEntity = _db.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (userRoleEntity == null)
            {
                userRoleEntity = new Role { RoleName = "User", CreatedAt = DateTime.UtcNow };
                _db.Roles.Add(userRoleEntity);
                await _db.SaveChangesAsync();
            }

            // Admin special number check
            var adminNumber = _cfg["ADMIN_MOBILE"] ?? Environment.GetEnvironmentVariable("ADMIN_MOBILE") ?? "7696609876";
            Role assignRole = userRoleEntity;
            if (!string.IsNullOrWhiteSpace(normalizedMobile) && normalizedMobile == adminNumber)
            {
                var adminRoleEntity = _db.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                if (adminRoleEntity == null)
                {
                    adminRoleEntity = new Role { RoleName = "Admin", CreatedAt = DateTime.UtcNow };
                    _db.Roles.Add(adminRoleEntity);
                    await _db.SaveChangesAsync();
                }
                assignRole = adminRoleEntity;
            }

            // assign role
            var userRole = new UserRole { UserId = user.UserId, RoleId = assignRole.RoleId, CreatedAt = DateTime.UtcNow };
            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();
        }
        else
        {
            // ensure existing user has at least 'User' role
            var hasRole = _db.UserRoles.Any(ur => ur.UserId == user.UserId);
            if (!hasRole)
            {
                var userRoleEntity = _db.Roles.FirstOrDefault(r => r.RoleName == "User");
                if (userRoleEntity == null)
                {
                    userRoleEntity = new Role { RoleName = "User", CreatedAt = DateTime.UtcNow };
                    _db.Roles.Add(userRoleEntity);
                    await _db.SaveChangesAsync();
                }
                _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = userRoleEntity.RoleId, CreatedAt = DateTime.UtcNow });
                await _db.SaveChangesAsync();
            }
        }

        var rnd = new Random();
        string otp = rnd.Next(100000, 999999).ToString();

        var otplog = new Otplog
        {
            UserId = user.UserId,
            MobileNumber = request.MobileNumber ?? request.Email ?? string.Empty,
            Otpcode = otp,
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5)
        };

        _db.Otplogs.Add(otplog);
        await _db.SaveChangesAsync();

        // Send OTP via SMS or Email depending on available contact
        var returnOtpInDev = (_cfg["RETURN_OTP_DEV"] ?? Environment.GetEnvironmentVariable("RETURN_OTP_DEV")) == "true";
        if (!string.IsNullOrWhiteSpace(normalizedMobile))
        {
            var to = "+91" + normalizedMobile;
            await _smsSender.SendSmsAsync(to, $"Your MediKart OTP is: {otp}");
        }
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            await _emailSender.SendEmailAsync(request.Email!, "MediKart OTP", $"Your OTP is <b>{otp}</b>");
        }

        return new AuthResultDto
        {
            Success = true,
            Message = "OTP generated",
            Otp = returnOtpInDev ? otp : null
        };
    }

public async Task<AuthResultDto> VerifyOtpAsync(VerifyOtpRequest request)
{
    if (string.IsNullOrWhiteSpace(request.MobileNumber) && string.IsNullOrWhiteSpace(request.Email))
        return new AuthResultDto { Success = false, Message = "Provide mobileNumber or email" };

    string identifier = request.MobileNumber ?? request.Email!;

    var otplog = _db.Otplogs
        .Where(o => o.MobileNumber == identifier && o.IsUsed == false)
        .OrderByDescending(o => o.CreatedAt)
        .FirstOrDefault();

    if (otplog == null)
        return new AuthResultDto { Success = false, Message = "OTP not found or already used" };

    if (otplog.ExpiryTime < DateTime.UtcNow)
        return new AuthResultDto { Success = false, Message = "OTP expired" };

    if (otplog.Otpcode != request.OtpCode)
    {
        otplog.AttemptCount = (otplog.AttemptCount ?? 0) + 1;
        otplog.AttemptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AuthResultDto { Success = false, Message = "Invalid OTP" };
    }

    // ✅ Mark OTP used
    otplog.IsUsed = true;
    otplog.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    // ✅ Get user
    var user = await _db.Users.FindAsync(otplog.UserId);

    if (user == null)
        return new AuthResultDto { Success = false, Message = "User not found" };

    // ✅ Update user
    user.IsMobileVerified = true;
    user.LastLoginAt = DateTime.UtcNow;
    user.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    // ✅ Get roles
    var roles = _db.UserRoles
        .Where(ur => ur.UserId == user.UserId)
        .Select(ur => ur.Role.RoleName)
        .ToArray();

    // ================= JWT =================
    string jwtKey = _cfg["JWT_KEY"] ?? Environment.GetEnvironmentVariable("JWT_KEY")!;
    string jwtIssuer = _cfg["JWT_ISSUER"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER")!;
    string jwtAudience = _cfg["JWT_AUDIENCE"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
    int jwtExpires = int.Parse(_cfg["JWT_EXPIRES_MINUTES"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES") ?? "60");

    var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // ✅ UserId in JWT
        new Claim(ClaimTypes.MobilePhone, user.MobileNumber ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? "")
    };

    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(jwtExpires),
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature
        )
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    // ✅ FINAL RESPONSE WITH USERID
    return new AuthResultDto
    {
        Success = true,
        Message = "Login successful",
        Token = tokenString,
        ExpiresInMinutes = jwtExpires,
        Roles = roles,
        UserId = user.UserId  
    };
}
}
