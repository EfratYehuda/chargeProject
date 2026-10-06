using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL
{
    public class ChargingStations
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OwnerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public string ChargerType { get; set; }
        [Required]
        public double MaxKw { get; set; }
        [Required]
        public double PricePerKwh { get; set; }
        [Required]
        public bool IsActive { get; set; }

        public Owner Owner { get; set; } // הבעלים של העמדה
        public List<ChargeBooking> Bookings { get; set; } // הזמנות בעמדה
        public List<StationStatusLogs> Logs { get; set; } // לוגים של סטטוסים
    }
}
