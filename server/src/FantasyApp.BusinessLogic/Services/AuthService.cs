using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyApp.BusinessLogic.Interfaces;
using FantasyApp.Common.Interfaces;
using FantasyApp.Common.Settings;
using FantasyApp.Entity.Dtos.Auth;
using FantasyApp.Entity.Models;
using FantasyApp.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FantasyApp.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly AppSettings _appSettings;
        private readonly IFantasyTeamRepository _fantasyTeamRepository;
        private readonly ILeagueRepository _leagueRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailSender emailSender,
            IOptions<AppSettings> appSettings,
            IFantasyTeamRepository fantasyTeamRepository,
            ILeagueRepository leagueRepository)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _emailSender = emailSender;
            _appSettings = appSettings.Value;
            _fantasyTeamRepository = fantasyTeamRepository;
            _leagueRepository = leagueRepository;
        }

        
        public async Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return AuthResult<AuthResponseDto>.Failure("A user with this email already exists.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Username
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
                return AuthResult<AuthResponseDto>.Failure(errors);
            }

            await _fantasyTeamRepository.AddAsync(new FantasyTeam
            {
                UserId = user.Id,
                Name = $"{user.UserName}'s Team",
                BudgetRemainingTenths = 1000,
                FreeTransfersAvailable = 1
            });
            await _fantasyTeamRepository.SaveChangesAsync();

            var officialLeague = await _leagueRepository.GetOfficialLeagueAsync();
            if (officialLeague != null)
            {
                await _leagueRepository.AddMembershipAsync(new LeagueMembership
                {
                    LeagueId = officialLeague.Id,
                    UserId = user.Id,
                    JoinedAt = DateTime.UtcNow
                });
                await _leagueRepository.SaveChangesAsync();
            }

            var authResponse = await IssueTokensAsync(user);
            return AuthResult<AuthResponseDto>.Success(authResponse);
        }

        public async Task<AuthResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return AuthResult<AuthResponseDto>.Failure("Incorrect email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return AuthResult<AuthResponseDto>.Failure("Incorrect email or password.");
            }

            var authResponse = await IssueTokensAsync(user);
            return AuthResult<AuthResponseDto>.Success(authResponse);
        }

        public async Task<AuthResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (existingToken == null || !existingToken.IsActive || existingToken.User == null)
            {
                return AuthResult<AuthResponseDto>.Failure("Refresh token is invalid or has expired.");
            }

            var (newRefreshTokenValue, newRefreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByToken = newRefreshTokenValue;

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existingToken.UserId,
                Token = newRefreshTokenValue,
                ExpiresAt = newRefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(existingToken.User);

            var response = new AuthResponseDto
            {
                UserId = existingToken.User.Id,
                Email = existingToken.User.Email ?? string.Empty,
                Username = existingToken.User.UserName ?? string.Empty,
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = newRefreshTokenValue,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt
            };

            return AuthResult<AuthResponseDto>.Success(response);
        }

        public async Task<AuthResult<bool>> RevokeTokenAsync(RefreshTokenRequestDto request)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (existingToken == null || !existingToken.IsActive)
            {
                return AuthResult<bool>.Failure("Refresh token is invalid or has already been revoked.");
            }

            existingToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();

            return AuthResult<bool>.Success(true);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(resetToken);
            var resetLink = $"{_appSettings.ClientUrl}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={encodedToken}";

            var htmlBody = $"""
                <p>Hi {user.UserName},</p>
                <p>We received a request to reset the password for your Fantasy account. Click the link below to set a new password:</p>
                <p><a href="{resetLink}">{resetLink}</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                """;

            await _emailSender.SendEmailAsync(request.Email, "Reset your password - Fantasy", htmlBody);
        }

        public async Task<AuthResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return AuthResult<bool>.Failure("Invalid password reset request.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return AuthResult<bool>.Failure(errors);
            }

            return AuthResult<bool>.Success(true);
        }

        private async Task<AuthResponseDto> IssueTokensAsync(ApplicationUser user)
        {
            var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user);
            var (refreshTokenValue, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }
    }
}
