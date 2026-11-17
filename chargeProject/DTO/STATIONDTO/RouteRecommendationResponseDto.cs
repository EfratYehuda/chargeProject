using System.Collections.Generic;

namespace DTO.STATIONDTO
{
    /// <summary>תשובת מצב מסלול: האם צריך לעצור, ואם כן – העמדות המומלצות בדרך</summary>
    public class RouteRecommendationResponseDto
    {
        public bool NoChargeNeeded { get; set; }        // true = הסוללה מספיקה עד היעד
        public double TripKm { get; set; }              // אורך הנסיעה המוערך (כביש)
        public double RangeKm { get; set; }             // הטווח של הסוללה הנוכחית (אחרי רזרבה)
        public int? ArrivalPercentAtDestination { get; set; } // רק כשאין צורך בטעינה
        public string Message { get; set; } = "";       // משפט סיכום למשתמש
        public List<RankedStationResultDto> Stations { get; set; } = new(); // ממוינות מהטובה לפחות טובה
    }
}
