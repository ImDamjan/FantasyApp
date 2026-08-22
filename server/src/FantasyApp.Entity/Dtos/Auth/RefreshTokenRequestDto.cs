using System.ComponentModel.DataAnnotations;

namespace FantasyApp.Entity.Dtos.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
