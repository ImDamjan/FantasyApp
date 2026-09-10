namespace FantasyApp.Entity.Dtos.Leagues
{
    public class LeagueDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }
        public int MemberCount { get; set; }
    }
}
