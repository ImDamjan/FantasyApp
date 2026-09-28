namespace FantasyApp.Entity.Models
{
    public class SquadPlayer
    {
        public long Id { get; set; }

        public long FantasyTeamId { get; set; }
        public FantasyTeam? FantasyTeam { get; set; }

        public long PlayerId { get; set; }
        public Player? Player { get; set; }

        public bool IsStarting { get; set; }
        public int? BenchOrder { get; set; }

        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }

        public int PurchasePriceTenths { get; set; }
    }
}
