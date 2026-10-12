using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    public class LoginRequestDto
    {
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>בחירת תפקיד במסך הכניסה: Driver / Owner</summary>
        [Required, RegularExpression("^(Driver|Owner)$", ErrorMessage = "תפקיד חייב להיות Driver או Owner")]
        public string Role { get; set; } = string.Empty;
    }
}
