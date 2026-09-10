using System;

namespace FantasyApp.Entity.Models
{
    public class Gameweek
    {
        public long Id { get; set; }
        public int FplId { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime DeadlineTime { get; set; }

        public bool IsCurrent { get; set; }
        public bool IsNext { get; set; }
        public bool IsFinished { get; set; }
    }
}
