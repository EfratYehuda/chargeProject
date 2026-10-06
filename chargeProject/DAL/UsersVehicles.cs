using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL
{
    public class UsersVehicles
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public double BatteryCapacityKwh { get; set; }

        [Required]
        public double CurrentBattery { get; set; }

        [Required]
        public string SupportedChargerType { get; set; }

        public Drivers Driver { get; set; } // הנהג של הרכב
        public List<ChargeBooking> Bookings { get; set; } // היסטוריית הטעינות של הרכב
    }
}
