using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.STATIONDTO
{
    public class StationDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ChargerType { get; set; } = string.Empty;
        public double MaxKw { get; set; }
        public double PricePerKwh { get; set; }
        public bool IsActive { get; set; }
        public double? AverageRating { get; set; }   // null אם אין ביקורות
        public int ReviewsCount { get; set; }
        public string? CurrentStatus { get; set; }   // הסטטוס האחרון מהלוג
    }

}
