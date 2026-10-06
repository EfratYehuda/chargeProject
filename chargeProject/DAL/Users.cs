using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]

         public string Name { get; set; }

        [Required]

        public string Email { get; set; }

        [Required]

        public string PasswordHash { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Access { get; set; }

        public Drivers DriverProfile { get; set; }
        public Owner OwnerProfile { get; set; }
    }
}
