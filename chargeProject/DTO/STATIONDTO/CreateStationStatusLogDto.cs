using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.STATIONDTO
{
    public class CreateStationStatusLogDto : IValidatableObject
    {
        [Required] public int StationId { get; set; }

        [Required, RegularExpression(StationStatuses.Pattern, ErrorMessage = "סטטוס לא חוקי")]
        public string Status { get; set; } = string.Empty;

        /// <summary>הנהג הנוכחי / המדווח (חובה כשהסטטוס Occupied או Faulty)</summary>
        public int? CurrentDriverId { get; set; }
        // המחלקה יורשת וילדציות שקיימות היום באפן אוטמטי בויזואל חלק מן הוילדציות הם: זמן ההגעה הנבחר הוא לא של אתמול אחוזי הסוללה גבוהים מהנוכחי וכו 
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((Status == StationStatuses.Occupied || Status == StationStatuses.Faulty) && CurrentDriverId == null)
                yield return new ValidationResult("יש לציין נהג עבור סטטוס תפוס/תקלה",
                    new[] { nameof(CurrentDriverId) });
        }
    }
}
