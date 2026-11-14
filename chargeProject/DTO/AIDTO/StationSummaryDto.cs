using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.AIDTO
{
    public class StationSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ChargerType { get; set; } = "";
        public double MaxKw { get; set; }
        public double PricePerKwh { get; set; }
    }
}
