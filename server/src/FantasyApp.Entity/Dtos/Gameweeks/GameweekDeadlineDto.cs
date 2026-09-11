using System;

namespace FantasyApp.Entity.Dtos.Gameweeks
{
    public class GameweekDeadlineDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime DeadlineTime { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsNext { get; set; }
    }
}
