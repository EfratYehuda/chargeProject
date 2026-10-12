using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.STATIONDTO
{
    /// <summary>רשומת סטטוס של עמדה (כולל דיווחי תקלה)</summary>
    public class StationStatusLogDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CurrentDriverId { get; set; }
        public string? CurrentDriverName { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
