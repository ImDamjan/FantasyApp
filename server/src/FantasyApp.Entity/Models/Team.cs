using System.Collections.Generic;

namespace FantasyApp.Entity.Models
{
    public class Team
    {
        public long Id { get; set; }
        public int FplId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

        public int StrengthOverallHome { get; set; }
        public int StrengthOverallAway { get; set; }

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
