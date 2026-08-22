using System.ComponentModel.DataAnnotations;

namespace FantasyApp.Entity.Dtos.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
