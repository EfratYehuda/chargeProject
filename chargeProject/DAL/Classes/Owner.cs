using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Classes
{
    public class Owner
    {
        [Key]

        public int Id { get; set; }
        public int UserId { get; set; } // מפתח זר למשתמש
        public Users User { get; set; } // קשר למשתמש
        public List<ChargingStations> Stations { get; set; } // העמדות שלו

    }

}
   