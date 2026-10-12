using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.VEHICLESDTO
{
    public class CreateChargeBookingDto
    {
        [Required] public int StationId { get; set; }
        [Required] public int DriverId { get; set; }
        [Required] public int VehicleId { get; set; }

        [Range(0, 100)]
        public double CurrentBatteryPercent { get; set; }

        [Range(1, 100)]
        public double TargetBatteryPercent { get; set; }

        [Required]
        public DateTime EstimatedArrivalTime { get; set; }
    }
}
