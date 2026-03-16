using System.Threading.Tasks;

namespace MediKartX.Application.Interfaces;

public interface ISmsSender
{
    Task<bool> SendSmsAsync(string to, string message);
}

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlContent);
}
