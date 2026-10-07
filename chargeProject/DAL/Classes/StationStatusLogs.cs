using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Classes
{
    public class StationStatusLogs
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StationId { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public int CurrentDriverId { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ChargingStations Station { get; set; } // העמדה המדוברת
        public Drivers CurrentDriver { get; set; } // הנהג הנוכחי
    }
}
