using System.ComponentModel.DataAnnotations;

namespace DAL.Classes
{
    public class ChargeBooking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int StationId { get; set; }
        [Required]
        public int DriverId { get; set; }
        [Required]
        public int VehicleId { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public double CurrentBatteryPercent { get; set; }
        [Required]
        public double TargetBatteryPercent { get; set; }
        [Required]
        public DateTime EstimatedArrivalTime { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ChargingStations Station { get; set; }
        public Drivers Driver { get; set; }
        public UsersVehicles Vehicle { get; set; }
        public ReviewsToCharge Review { get; set; } // אם יש ביקורת להזמנה
    }
}
