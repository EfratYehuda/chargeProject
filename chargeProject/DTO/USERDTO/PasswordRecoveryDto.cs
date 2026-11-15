using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordResultDto
    {
        public bool SentViaEmail { get; set; }
        public string? DevCode { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain letters and digits.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
