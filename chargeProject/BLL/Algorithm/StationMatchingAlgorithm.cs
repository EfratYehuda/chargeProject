using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Algorithm
{
    // נתוני הבקשה וסדרי העדיפויות של הנהג
    public class DriverPreferenceRequest
    {
        public double DriverLatitude { get; set; }
        public double DriverLongitude { get; set; }
        public int BatteryPercentage { get; set; } = 50; // אחוז סוללה נוכחי
        public int AvailableTimeMinutes { get; set; } = 60; // זמן פנוי בדקות (למשל: 120 לשעתיים)

        // סינוני חובה:
        public string? RequiredChargerType { get; set; } // סוג שקע מבוקש (אם הנהג הגדיר)
        public double? MinRating { get; set; } // דירוג מינימלי שהנהג דורש (למשל 4.0)

        // משקולות העדפה באחוזים (סכומם 1.0 = 100%):
        public double WeightDistance { get; set; } = 0.25; // 25% קרבה
        public double WeightRating { get; set; } = 0.30;   // 30% דירוג ואיכות העמדה
        public double WeightPrice { get; set; } = 0.25;    // 25% מחיר זול
        public double WeightSpeed { get; set; } = 0.20;    // 20% התאמת זמן והספק
    }

    // תוצאת שקלול העמדה
    public class RankedStationResult
    {
        public StationDto Station { get; set; } = null!;
        public double DistanceKm { get; set; }
        public int MatchPercentage { get; set; } // אחוז התאמה (0 עד 100%)
        public string MatchReason { get; set; } = "";
        public bool IsEmergencyMode { get; set; }
    }

    public class StationMatchingAlgorithm
    {
        public List<RankedStationResult> RankStations(
            List<StationDto> stations,
            DriverPreferenceRequest pref)
        {
            var results = new List<RankedStationResult>();

            // הערכת טווח נסיעה נותר: כ-3 ק"מ לכל 1% סוללה
            double estimatedRangeKm = Math.Max(pref.BatteryPercentage * 3.0, 1.0);
            bool isEmergency = pref.BatteryPercentage <= 10; // האם במצב סוללה קריטית?

            foreach (var st in stations)
            {
                // 1. חישוב מרחק מדויק מהנהג לעמדה בק"מ
                double dist = CalculateDistanceKm(pref.DriverLatitude, pref.DriverLongitude, st.Latitude, st.Longitude);

                // 2. חוק ברזל: אם העמדה מחוץ לטווח הסוללה - לא ניתן להגיע אליה!
                if (dist > estimatedRangeKm)
                {
                    continue; // פסילת עמדה מחוץ לטווח
                }

                // 3. סינון סוג שקע (אם הנהג ביקש שקע ספציפי)
                if (!string.IsNullOrWhiteSpace(pref.RequiredChargerType) &&
                    !st.ChargerType.Contains(pref.RequiredChargerType, StringComparison.OrdinalIgnoreCase))
                {
                    continue; // שקע לא מתאים לרכב
                }

                // 4. בדיקת דירוג העמדה:
                double stationRating = st.AverageRating ?? 4.0;

                // סינון עמדות תקולות
                if (!string.IsNullOrWhiteSpace(st.CurrentStatus) &&
                    (st.CurrentStatus.Contains("תקול") || st.CurrentStatus.Contains("Fault") || st.CurrentStatus.Contains("Broken")))
                {
                    continue;
                }

                // סינון דירוג מינימלי (אם נדרש ולא במצב חירום)
                if (pref.MinRating.HasValue && stationRating < pref.MinRating.Value && !isEmergency)
                {
                    continue;
                }

                double finalScore = 0;
                string reason = "";

                if (isEmergency)
                {
                    // === מצב חירום: סוללה קריטית! ===
                    double distScore = Math.Max(0, 100 - (dist * 10));
                    finalScore = distScore;
                    reason = $"🚨 מצב חירום (סוללה {pref.BatteryPercentage}%): זו העמדה הקרובה ביותר שתוכל להגיע אליה בבטחה ({dist:F1} ק\"מ).";
                }
                else
                {
                    // === מצב רגיל: שקלול העדפות ===
                    double distScore = Math.Max(0, 100 - (dist * 5));
                    double ratingScore = Math.Clamp((stationRating / 5.0) * 100, 0, 100);
                    double priceScore = Math.Max(0, 100 - (st.PricePerKwh * 30));

                    double speedScore = 50;
                    if (pref.AvailableTimeMinutes >= 90)
                    {
                        speedScore = (st.MaxKw <= 25) ? 100 : 70;
                    }
                    else if (pref.AvailableTimeMinutes <= 45)
                    {
                        speedScore = (st.MaxKw >= 50) ? 100 : 30;
                    }

                    finalScore = (distScore * pref.WeightDistance) +
                                 (ratingScore * pref.WeightRating) +
                                 (priceScore * pref.WeightPrice) +
                                 (speedScore * pref.WeightSpeed);

                    reason = $"התאמה של {Math.Round(finalScore)}%: עמדה במרחק {dist:F1} ק\"מ, דירוג ⭐{stationRating:F1}, מחיר {st.PricePerKwh} ₪/קוט\"ש והספק {st.MaxKw}kW.";
                }

                results.Add(new RankedStationResult
                {
                    Station = st,
                    DistanceKm = Math.Round(dist, 2),
                    MatchPercentage = (int)Math.Clamp(Math.Round(finalScore), 1, 99),
                    MatchReason = reason,
                    IsEmergencyMode = isEmergency
                });
            }

            // הצלת חירום: אם כל העמדות היו מחוץ לטווח הסוללה במצב חירום - מחזירים את הקרובה ביותר עם אזהרה חמורה
            if (isEmergency && results.Count == 0 && stations.Count > 0)
            {
                var nearest = stations
                    .Select(s => new { Station = s, Dist = CalculateDistanceKm(pref.DriverLatitude, pref.DriverLongitude, s.Latitude, s.Longitude) })
                    .OrderBy(x => x.Dist)
                    .First();

                results.Add(new RankedStationResult
                {
                    Station = nearest.Station,
                    DistanceKm = Math.Round(nearest.Dist, 2),
                    MatchPercentage = 40,
                    MatchReason = $"🚨 סכנת היתקעות! העמדה הקרובה ביותר נמצאת במרחק {nearest.Dist:F1} ק\"מ (מעבר לטווח המוערך שלך של {estimatedRangeKm:F1} ק\"מ). כבה מזגן וסע מיד במצב חיסכון!",
                    IsEmergencyMode = true
                });
            }

            // מיון סופי מהציון הגבוה לנמוך
            return results.OrderByDescending(r => r.MatchPercentage).ToList();
        }

        private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // ק"מ
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }
    }
}