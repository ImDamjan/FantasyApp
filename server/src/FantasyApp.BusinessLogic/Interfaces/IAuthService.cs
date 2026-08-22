using System.Threading.Tasks;
using FantasyApp.Entity.Dtos.Auth;

namespace FantasyApp.BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<AuthResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<AuthResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<AuthResult<bool>> RevokeTokenAsync(RefreshTokenRequestDto request);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<AuthResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }

    public class AuthResult<T>
    {
        public bool Succeeded { get; private init; }
        public T? Data { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static AuthResult<T> Success(T data) => new() { Succeeded = true, Data = data };
        public static AuthResult<T> Failure(string errorMessage) => new() { Succeeded = false, ErrorMessage = errorMessage };
    }
}
