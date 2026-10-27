using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.STATIONDTO
{
    /// <summary>תוצאה מומלצת (אחרי מיון ב-Merge Sort בשכבת השירות)</summary>
    public class StationRecommendationDto
    {
        public StationDto Station { get; set; } = new();
        public double DistanceKm { get; set; }
        public double Score { get; set; }
    }
}
