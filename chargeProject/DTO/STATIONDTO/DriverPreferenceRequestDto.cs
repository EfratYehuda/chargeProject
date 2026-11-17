namespace DTO.STATIONDTO
{
    /// <summary>בקשת המלצה מהנהג: מיקום, סוללה, העדפות, ויעד אופציונלי</summary>
    public class DriverPreferenceRequestDto
    {
        public double DriverLatitude { get; set; }
        public double DriverLongitude { get; set; }
        public int BatteryPercentage { get; set; } = 50; // אחוז סוללה נוכחי
        public int AvailableTimeMinutes { get; set; } = 60; // זמן פנוי בדקות

        // === מצב מסלול (אופציונלי): אם נשלח יעד – מחפשים עצירה בדרך ===
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public double BatteryCapacityKwh { get; set; } = RouteRules.DefaultBatteryCapacityKwh; // קיבולת סוללת הרכב שנבחר
        public bool HasDestination => DestinationLatitude.HasValue && DestinationLongitude.HasValue;

        // סינוני חובה:
        public string? RequiredChargerType { get; set; } // סוג שקע מבוקש
        public double? MinRating { get; set; }            // דירוג מינימלי

        // משקולות העדפה – מצב רדיוס (סכומם 1.0):
        public double WeightDistance { get; set; } = 0.25;
        public double WeightRating { get; set; } = 0.30;
        public double WeightPrice { get; set; } = 0.25;
        public double WeightSpeed { get; set; } = 0.20;
    }
}
