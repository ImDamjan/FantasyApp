namespace FantasyApp.Entity.Dtos.Points
{
    public class PointsHistoryItemDto
    {
        public string GameweekName { get; set; } = string.Empty;
        public int RawPoints { get; set; }
        public int TransferCost { get; set; }
        public int NetPoints { get; set; }
        public string? ChipUsed { get; set; }
        public bool IsFinal { get; set; }
    }
}
