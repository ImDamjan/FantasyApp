using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Auth;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ServiceResult<bool>> RevokeTokenAsync(RefreshTokenRequestDto request);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
