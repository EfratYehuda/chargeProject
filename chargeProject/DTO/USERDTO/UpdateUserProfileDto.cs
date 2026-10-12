using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    public class UpdateUserProfileDto
    {
        [Required, StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, RegularExpression(@"^0\d{1,2}-?\d{7}$", ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; } = string.Empty;
    }
}
