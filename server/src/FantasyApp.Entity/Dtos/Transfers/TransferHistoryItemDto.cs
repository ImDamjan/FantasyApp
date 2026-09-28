using System;

namespace FantasyApp.Entity.Dtos.Transfers
{
    public class TransferHistoryItemDto
    {
        public string GameweekName { get; set; } = string.Empty;
        public string PlayerOutName { get; set; } = string.Empty;
        public string PlayerInName { get; set; } = string.Empty;
        public bool WasFreeTransfer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
