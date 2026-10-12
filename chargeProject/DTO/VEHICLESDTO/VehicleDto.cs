using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.VEHICLESDTO
{
    /// <summary>רכב של נהג (מסך פרופיל)</summary>
    public class VehicleDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string Model { get; set; } = string.Empty;
        public double BatteryCapacityKwh { get; set; }
        public double CurrentBattery { get; set; }          // באחוזים
        public string SupportedChargerType { get; set; } = string.Empty;
    }

}
