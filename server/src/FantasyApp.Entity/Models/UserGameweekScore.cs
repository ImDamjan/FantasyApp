namespace FantasyApp.Entity.Models
{
    public class UserGameweekScore
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public long GameweekId { get; set; }
        public Gameweek? Gameweek { get; set; }

        public int RawPoints { get; set; }
        public int TransferCost { get; set; }
        public int NetPoints { get; set; }

        public ChipType? ChipUsed { get; set; }
        public bool IsFinal { get; set; }
    }
}
