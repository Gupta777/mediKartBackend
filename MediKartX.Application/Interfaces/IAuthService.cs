using System.Threading.Tasks;
using MediKartX.Application.DTOs;

namespace MediKartX.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RequestOtpAsync(RequestOtpRequest request);
    Task<AuthResultDto> VerifyOtpAsync(VerifyOtpRequest request);
}
