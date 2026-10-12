using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.REVIEWDTO
{
    /// <summary>ביקורת להצגה (בעל עמדה / פרטי עמדה)</summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ChargeBookingId { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
