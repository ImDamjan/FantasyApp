using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FantasyApp.Entity.Dtos.Transfers
{
    public class TransferItemDto
    {
        public long PlayerOutId { get; set; }
        public long PlayerInId { get; set; }
    }

    public class SubmitTransfersRequestDto
    {
        [Required]
        public List<TransferItemDto> Transfers { get; set; } = new();
    }
}
