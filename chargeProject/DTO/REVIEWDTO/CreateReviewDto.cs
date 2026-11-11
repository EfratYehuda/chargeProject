using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.REVIEWDTO
{
    /// <summary>כתיבת ביקורת אחרי טעינה</summary>
    public class CreateReviewDto
    {
        [Required] public int ChargeBookingId { get; set; }

        // DriverId was removed: the server takes it from the token (driverId claim).

        [Range(1, 5)]
        public double Rating { get; set; }

        [Required(AllowEmptyStrings = false), StringLength(1000, MinimumLength = 1)]
        public string Comment { get; set; } = string.Empty;
    }
}