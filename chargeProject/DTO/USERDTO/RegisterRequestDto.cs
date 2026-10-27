using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    public class RegisterRequestDto
    {
        [Required, StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Israeli phone, e.g. 050-1234567 / 0501234567 / 03-1234567
        [Required, RegularExpression(@"^0\d{1,2}-?\d{7}$", ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain letters and digits.")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>בחירת תפקיד במסך הכניסה: Driver / Owner</summary>
        [Required, RegularExpression("^(Driver|Owner)$", ErrorMessage = "תפקיד חייב להיות Driver או Owner")]
        public string Role { get; set; } = string.Empty;
    }
}
