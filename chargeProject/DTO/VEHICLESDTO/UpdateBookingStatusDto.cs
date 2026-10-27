using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.VEHICLESDTO
{
    public class UpdateBookingStatusDto
    {
        //[Required, RegularExpression(BookingStatuses.Pattern, ErrorMessage = "סטטוס לא חוקי")]
        public string Status { get; set; } = string.Empty;
    }
}
