using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MediKartX.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MediKartX.Infrastructure.Services;

public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;

    public SendGridEmailSender(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent)
    {
        var apiKey = _cfg["SENDGRID_API_KEY"] ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var from = _cfg["SENDGRID_FROM"] ?? Environment.GetEnvironmentVariable("SENDGRID_FROM");

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
        {
            Console.WriteLine($"[SendGridDev] To:{to} Subject:{subject} Body:{htmlContent}");
            return await Task.FromResult(true);
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = $"{{\"personalizations\":[{{\"to\":[{{\"email\":\"{to}\"}}]}}],\"from\":{{\"email\":\"{from}\"}},\"subject\":\"{subject}\",\"content\":[{{\"type\":\"text/html\",\"value\":\"{htmlContent.Replace("\"","\\\"")}\"}}]}}";

        var resp = await client.PostAsync("https://api.sendgrid.com/v3/mail/send", new StringContent(payload, Encoding.UTF8, "application/json"));
        return resp.IsSuccessStatusCode;
    }
}
