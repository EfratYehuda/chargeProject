using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.STATIONDTO
{
    /// <summary>יצירה / עדכון עמדה (בעל עמדה)</summary>
    public class CreateUpdateStationDto
    {
        [Required]
        public int OwnerId { get; set; }

        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(250, MinimumLength = 3)]
        public string Address { get; set; } = string.Empty;

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required, RegularExpression(ChargerTypes.Pattern, ErrorMessage = "סוג מטען חייב להיות CCS או Type2")]
        public string ChargerType { get; set; } = string.Empty;

        [Range(1, 1000)]
        public double MaxKw { get; set; }

        [Range(0.01, 100)]
        public double PricePerKwh { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
