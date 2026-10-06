using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL
{
    public class ReviewsToCharge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChargeBookingId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public double Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ChargeBooking Booking { get; set; } // ההזמנה עליה נכתבה הביקורת
        public Drivers Driver { get; set; } // הנהג שכתב
    }
}
