using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.VEHICLESDTO
{
    /// <summary>יצירה / עדכון רכב</summary>
    public class CreateUpdateVehicleDto
    {
        [Required]
        public int DriverId { get; set; }

        [Required, StringLength(100, MinimumLength = 2)]
        public string Model { get; set; } = string.Empty;

        [Range(1, 300)]
        public double BatteryCapacityKwh { get; set; }

        [Range(0, 100)]
        public double CurrentBattery { get; set; }

        //[Required, RegularExpression(ChargerTypes.Pattern, ErrorMessage = "סוג מטען חייב להיות CCS או Type2")]
        public string SupportedChargerType { get; set; } = string.Empty;
    }
}
