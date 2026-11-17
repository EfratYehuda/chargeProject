using DTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Algorithm
{
    /// <summary>
    /// מצב מסלול: הנהג נותן מוצא, יעד ואחוז סוללה – והאלגוריתם מוצא איפה הכי משתלם לעצור בדרך.
    /// (partial: זה המשך של אותה מחלקה StationMatchingAlgorithm, בקובץ נפרד)
    /// </summary>
    public partial class StationMatchingAlgorithm
    {
        public RouteRecommendationResponseDto RankRouteStops(
            List<StationDto> stations,
            DriverPreferenceRequestDto pref)
        {
            double capacityKwh = pref.BatteryCapacityKwh > 0
                ? pref.BatteryCapacityKwh
                : RouteRules.DefaultBatteryCapacityKwh;

            double oLat = pref.DriverLatitude, oLon = pref.DriverLongitude;           // O = מוצא
            double dLat = pref.DestinationLatitude!.Value, dLon = pref.DestinationLongitude!.Value; // D = יעד

            // ===== צעד 1: האם בכלל צריך לעצור? =====
            double startKwh = pref.BatteryPercentage / 100.0 * capacityKwh;              // כמה קוט"ש יש עכשיו
            double rangeKm = startKwh / RouteRules.ConsumptionKwhPerKm - RouteRules.ReserveKm; // כמה ק"מ אפשר לנסוע ולהשאיר רזרבה
            double tripKm = GeoHelper.RoadDistanceKm(oLat, oLon, dLat, dLon);            // אורך הנסיעה O→D

            var response = new RouteRecommendationResponseDto
            {
                TripKm = Math.Round(tripKm, 1),
                RangeKm = Math.Round(Math.Max(rangeKm, 0), 1)
            };

            if (rangeKm >= tripKm)
            {
                double arrivalKwh = startKwh - tripKm * RouteRules.ConsumptionKwhPerKm;
                int arrivalPercent = (int)Math.Round(arrivalKwh / capacityKwh * 100);

                response.NoChargeNeeded = true;
                response.ArrivalPercentAtDestination = arrivalPercent;
                response.Message = $"✅ אין צורך בטעינה: הנסיעה {tripKm:F0} ק\"מ, ותגיעו ליעד עם כ-{arrivalPercent}% סוללה.";
                return response;
            }

            // ===== צעדים 2–3: סינון וחישוב לכל עמדה =====
            var candidates = new List<RankedStationResultDto>();

            foreach (var st in stations)
            {
                if (!PassesBasicFilters(st, pref)) continue;   // שקע / תקלה / דירוג מינימלי
                if (st.MaxKw <= 0) continue;                   // בלי הספק אי אפשר לחשב זמן טעינה

                double toStationKm = GeoHelper.RoadDistanceKm(oLat, oLon, st.Latitude, st.Longitude); // O→S
                double toDestKm = GeoHelper.RoadDistanceKm(st.Latitude, st.Longitude, dLat, dLon);   // S→D

                // אילוץ 1: חייבים להגיע לעמדה עם הסוללה הנוכחית
                if (toStationKm > rangeKm) continue;

                // אילוץ 2: העצירה לא מוציאה אותנו יותר מדי מהדרך
                double detourKm = toStationKm + toDestKm - tripKm;
                if (detourKm > RouteRules.MaxDetourKm) continue;

                // כמה קוט"ש יהיו בסוללה כשנגיע לעמדה
                double arrivalAtStationKwh = startKwh - toStationKm * RouteRules.ConsumptionKwhPerKm;

                // כמה צריך לטעון (מינימום) כדי להגיע מהעמדה ליעד ולהשאיר רזרבה
                double neededKwh = (toDestKm + RouteRules.ReserveKm) * RouteRules.ConsumptionKwhPerKm;
                double chargeKwh = Math.Max(0, neededKwh - arrivalAtStationKwh);

                // אילוץ 3: אם גם סוללה מלאה לא מספיקה מהעמדה ליעד – העמדה לא רלוונטית
                if (arrivalAtStationKwh + chargeKwh > capacityKwh) continue;

                double chargeCost = chargeKwh * st.PricePerKwh;                         // ₪
                double chargeMinutes = chargeKwh / st.MaxKw * 60;                       // קוט"ש ÷ קוט"ש-לשעה = שעות → דקות
                double detourMinutes = Math.Max(0, detourKm) / RouteRules.AverageSpeedKmh * 60;
                double extraMinutes = detourMinutes + chargeMinutes;

                double arrivalAtDestKwh = arrivalAtStationKwh + chargeKwh - toDestKm * RouteRules.ConsumptionKwhPerKm;

                candidates.Add(new RankedStationResultDto
                {
                    Station = st,
                    DistanceKm = Math.Round(toStationKm, 1),
                    DetourKm = Math.Round(Math.Max(0, detourKm), 1),
                    ChargeKwh = Math.Round(chargeKwh, 1),
                    ChargeCost = Math.Round(chargeCost, 1),
                    ChargeMinutes = Math.Round(chargeMinutes, 1),
                    ExtraMinutes = Math.Round(extraMinutes, 1),
                    ArrivalPercentAtDestination = (int)Math.Round(arrivalAtDestKwh / capacityKwh * 100)
                });
            }

            if (candidates.Count == 0)
            {
                response.Message = "⚠️ לא נמצאה עמדה מתאימה בדרך (בטווח הסוללה ועם עקיפה של עד " +
                                   $"{RouteRules.MaxDetourKm:F0} ק\"מ). נסו להרחיב את החיפוש או לטעון לפני היציאה.";
                return response;
            }

            // ===== צעד 4: ציון משוקלל (נרמול min/max בין המועמדות) =====
            // צריך את כל המועמדות לפני הציון, כי "זול" ו"מהיר" הם יחסיים לשאר העמדות
            double minTime = candidates.Min(c => c.ExtraMinutes!.Value), maxTime = candidates.Max(c => c.ExtraMinutes!.Value);
            double minCost = candidates.Min(c => c.ChargeCost!.Value), maxCost = candidates.Max(c => c.ChargeCost!.Value);

            foreach (var c in candidates)
            {
                double timeScore = 1 - Normalize(c.ExtraMinutes!.Value, minTime, maxTime); // הכי מהירה = 1
                double costScore = 1 - Normalize(c.ChargeCost!.Value, minCost, maxCost);   // הכי זולה = 1
                double ratingScore = (c.Station.AverageRating ?? 4.0) / 5.0;               // 5 כוכבים = 1

                double score = timeScore * RouteRules.WeightTime +
                               costScore * RouteRules.WeightCost +
                               ratingScore * RouteRules.WeightRating;

                c.MatchPercentage = (int)Math.Clamp(Math.Round(score * 100), 1, 100);
                c.MatchReason = BuildRouteReason(c);
            }

            // ===== צעד 5: מיון מהציון הגבוה לנמוך =====
            // TODO שלב 5: להחליף ב-Merge Sort שמימשנו
            response.Stations = candidates.OrderByDescending(c => c.MatchPercentage).ToList();
            response.Message = response.Stations[0].MatchReason;
            return response;
        }

        // ---------- פונקציות עזר ----------

        /// <summary>מנרמל ערך לטווח 0..1 ביחס למינימום ולמקסימום. אם כולם שווים – 0 (כלומר: כולם "הכי טובים")</summary>
        private static double Normalize(double value, double min, double max)
        {
            return max - min < 0.0001 ? 0 : (value - min) / (max - min);
        }

        /// <summary>סינונים בסיסיים: סוג שקע, עמדה תקולה, דירוג מינימלי</summary>
        private static bool PassesBasicFilters(StationDto st, DriverPreferenceRequestDto pref)
        {
            if (!string.IsNullOrWhiteSpace(pref.RequiredChargerType) &&
                !st.ChargerType.Contains(pref.RequiredChargerType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(st.CurrentStatus) &&
                (st.CurrentStatus.Contains("תקול") || st.CurrentStatus.Contains("Fault") || st.CurrentStatus.Contains("Broken")))
                return false;

            double rating = st.AverageRating ?? 4.0;
            if (pref.MinRating.HasValue && rating < pref.MinRating.Value)
                return false;

            return true;
        }

        /// <summary>משפט הסבר קריא, למשל: "עצרו ברמלה: 2.6 ק"מ עקיפה. מספיק לטעון 1.1 קוט"ש..."</summary>
        private static string BuildRouteReason(RankedStationResultDto c)
        {
            return $"עצירה מומלצת ב{c.Station.Name}: {c.DetourKm:F1} ק\"מ עקיפה. " +
                   $"מספיק לטעון {c.ChargeKwh:F1} קוט\"ש (~₪{c.ChargeCost:F1}, ~{Math.Ceiling(c.ExtraMinutes ?? 0)} דק' סה\"כ) " +
                   $"ותגיעו ליעד עם כ-{c.ArrivalPercentAtDestination}%. התאמה {c.MatchPercentage}%.";
        }
    }
}