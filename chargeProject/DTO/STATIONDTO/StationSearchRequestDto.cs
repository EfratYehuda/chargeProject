using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.STATIONDTO
{
    /// <summary>מסך חיפוש והעדפות של הנהג</summary>
    public class StationSearchRequestDto
    {
        [Range(-90, 90)]
        public double CurrentLatitude { get; set; }

        [Range(-180, 180)]
        public double CurrentLongitude { get; set; }

        [Range(0, 100)]
        public double CurrentBatteryPercent { get; set; }

        [Range(-90, 90)]
        public double? DestinationLatitude { get; set; }

        [Range(-180, 180)]
        public double? DestinationLongitude { get; set; }

        //[RegularExpression(ChargerTypes.Pattern)]
        public string? ChargerType { get; set; }          // סינון אופציונלי CCS / Type2

        /// <summary>משקל דירוג הקהילה באלגוריתם (0-1)</summary>
        [Range(0, 1)]
        public double CommunityRatingWeight { get; set; } = 0.5;

        /// <summary>משקל מהירות הטעינה באלגוריתם (0-1)</summary>
        [Range(0, 1)]
        public double SpeedWeight { get; set; } = 0.5;

        [Range(1, 500)]
        public double MaxDistanceKm { get; set; } = 30;

        [Range(1, 50)]
        public int MaxResults { get; set; } = 10;
    }
}
