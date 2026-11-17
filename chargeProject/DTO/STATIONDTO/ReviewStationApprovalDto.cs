using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.STATIONDTO
{

    /// <summary>בקשת מנהל לאישור או דחיית עמדה חדשה</summary>
    public class ReviewStationApprovalDto
    {
        [Required, RegularExpression("^(Approved|Rejected)$", ErrorMessage = "סטטוס חייב להיות Approved או Rejected")]
        public string ApprovalStatus { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? RejectionReason { get; set; }
    }
}
