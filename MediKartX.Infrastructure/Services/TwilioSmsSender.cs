using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MediKartX.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MediKartX.Infrastructure.Services;

public class TwilioSmsSender : ISmsSender
{
    private readonly IConfiguration _cfg;

    public TwilioSmsSender(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public async Task<bool> SendSmsAsync(string to, string message)
    {
        var accountSid = _cfg["TWILIO_ACCOUNT_SID"] ?? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        var authToken = _cfg["TWILIO_AUTH_TOKEN"] ?? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        var from = _cfg["TWILIO_FROM"] ?? Environment.GetEnvironmentVariable("TWILIO_FROM");

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(from))
        {
            // Dev fallback: just log to console
            Console.WriteLine($"[TwilioDev] To:{to} Message:{message}");
            return await Task.FromResult(true);
        }

        using var client = new HttpClient();
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

        var data = new[] {
            new KeyValuePair<string,string>("To", to),
            new KeyValuePair<string,string>("From", from),
            new KeyValuePair<string,string>("Body", message)
        };

        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(data) };
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var resp = await client.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }
}
