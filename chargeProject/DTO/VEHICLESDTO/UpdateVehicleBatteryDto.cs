using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.VEHICLESDTO
{
    public class UpdateVehicleBatteryDto
    {
        [Range(0, 100)]
        public double CurrentBattery { get; set; }
    }
}
