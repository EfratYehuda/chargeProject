using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    /// <summary>יצירת פרופיל נהג למשתמש קיים</summary>
    public class CreateDriverDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
