using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MediKartX.Infrastructure.Data;
using MediKartX.Infrastructure.Services;
using MediKartX.Application.DTOs;
using MediKartX.Application.Interfaces;

namespace MediKartX.Tests;

public class AuthServiceTests
{
    private MediKartXDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediKartXDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new MediKartXDbContext(options);
    }

    private IConfiguration CreateConfiguration()
    {
        var dict = new System.Collections.Generic.Dictionary<string, string?>();
        dict["RETURN_OTP_DEV"] = "true";
        dict["ADMIN_MOBILE"] = "7696609876";
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private class DummySms : ISmsSender { public Task<bool> SendSmsAsync(string to, string message) => Task.FromResult(true); }
    private class DummyEmail : IEmailSender { public Task<bool> SendEmailAsync(string to, string subject, string html) => Task.FromResult(true); }

    [Fact]
    public async Task RequestOtp_AssignsUserRole_And_AdminRoleForAdminNumber()
    {
        var db = CreateDbContext("test1");
        var cfg = CreateConfiguration();
        var sms = new DummySms();
        var email = new DummyEmail();
        var svc = new AuthService(db, cfg, sms, email);

        var res = await svc.RequestOtpAsync(new RequestOtpRequest { MobileNumber = "+917696609876" });
        Assert.True(res.Success);

        var user = await db.Users.FirstOrDefaultAsync(u => u.MobileNumber == "7696609876");
        Assert.NotNull(user);

        var roles = await db.UserRoles.Include(ur => ur.Role).ToListAsync();
        Assert.Contains(roles, r => r.Role.RoleName == "Admin");
    }

    [Fact]
    public async Task RequestOtp_InvalidIndianNumber_ReturnsError()
    {
        var db = CreateDbContext("test2");
        var cfg = CreateConfiguration();
        var sms = new DummySms();
        var email = new DummyEmail();
        var svc = new AuthService(db, cfg, sms, email);

        var res = await svc.RequestOtpAsync(new RequestOtpRequest { MobileNumber = "12345" });
        Assert.False(res.Success);
    }
}
