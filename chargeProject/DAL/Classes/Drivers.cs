using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Classes
{
    public class Drivers
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // מפתח זר למשתמש
        public Users User { get; set; } // קשר למשתמש
        public List<UsersVehicles> Vehicles { get; set; } // הרכבים שלו
        public List<ChargeBooking> Bookings { get; set; } // ההזמנות שלו
        public List<ReviewsToCharge> Reviews { get; set; } // הביקורות שלו
    }
}
