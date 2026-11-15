using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.VEHICLESDTO
{
    /// <summary>הזמנת טעינה להצגה (היסטוריה, שריון פעיל, דשבורד)</summary>
    public class ChargeBookingDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationAddress { get; set; } = string.Empty;
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleModel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double CurrentBatteryPercent { get; set; }
        public double TargetBatteryPercent { get; set; }
        public DateTime EstimatedArrivalTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasReview { get; set; }

        /// <summary>מחושב דינמית: מתי פג השריון (רק כשהסטטוס Held). אין setter ולכן AutoMapper מתעלם.</summary>
        public DateTime? HoldExpiresAt
        {
            get
            {
                if (Status != BookingStatuses.Held)
                    return null;

                if (EstimatedArrivalTime > CreatedAt &&
                    EstimatedArrivalTime <= CreatedAt.AddMinutes(BookingRules.MaxHoldMinutes))
                {
                    return EstimatedArrivalTime;
                }

                return CreatedAt.AddMinutes(BookingRules.HoldMinutes);
            }
        }
    }
}