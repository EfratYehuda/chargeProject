namespace DTO.STATIONDTO
{
    /// <summary>עמדה מדורגת שחוזרת ללקוח</summary>
    public class RankedStationResultDto
    {
        public StationDto Station { get; set; } = null!;
        public double DistanceKm { get; set; }
        public int MatchPercentage { get; set; } // 0–100
        public string MatchReason { get; set; } = "";
        public bool IsEmergencyMode { get; set; }

        // === שדות מצב מסלול (null כשאין יעד) ===
        public double? DetourKm { get; set; }
        public double? ChargeKwh { get; set; }
        public double? ChargeCost { get; set; }
        public double? ChargeMinutes { get; set; }
        public double? ExtraMinutes { get; set; }
        public int? ArrivalPercentAtDestination { get; set; }
    }
}
