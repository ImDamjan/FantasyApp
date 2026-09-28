using System.ComponentModel.DataAnnotations;

namespace FantasyApp.Entity.Dtos.Leagues
{
    public class CreateLeagueRequestDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;
    }

    public class JoinLeagueRequestDto
    {
        [Required]
        public string JoinCode { get; set; } = string.Empty;
    }
}
