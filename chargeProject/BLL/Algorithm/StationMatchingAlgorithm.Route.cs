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
        private const int MinComfortableArrivalPercent = 20;

        public RouteRecommendationResponseDto RankRouteStops(
            List<StationDto> stations,
            DriverPreferenceRequestDto pref)
        {
            double capacityKwh = pref.BatteryCapacityKwh > 0
                ? pref.BatteryCapacityKwh
                : RouteRules.DefaultBatteryCapacityKwh;

            double oLat = pref.DriverLatitude, oLon = pref.DriverLongitude;
            double dLat = pref.DestinationLatitude!.Value, dLon = pref.DestinationLongitude!.Value;

            // ===== צעד 1: האם הסוללה מספיקה לנסיעה ועם כמה מגיעים? =====
            double startKwh = pref.BatteryPercentage / 100.0 * capacityKwh;
            double minReserveKwh = RouteRules.ReserveKm * RouteRules.ConsumptionKwhPerKm;
            double rangeKm = startKwh / RouteRules.ConsumptionKwhPerKm - RouteRules.ReserveKm;
            double tripKm = GeoHelper.RoadDistanceKm(oLat, oLon, dLat, dLon);

            var response = new RouteRecommendationResponseDto
            {
                TripKm = Math.Round(tripKm, 1),
                RangeKm = Math.Round(Math.Max(rangeKm, 0), 1)
            };

            double directArrivalKwh = startKwh - tripKm * RouteRules.ConsumptionKwhPerKm;
            int directArrivalPercent = (int)Math.Round(directArrivalKwh / capacityKwh * 100);
            bool canReachPhysically = rangeKm >= tripKm;

            // רק אם מגיעים בבטחה וגם מעל 20% סוללה — באמת אין צורך לעצור
            if (canReachPhysically && directArrivalPercent >= MinComfortableArrivalPercent)
            {
                response.NoChargeNeeded = true;
                response.ArrivalPercentAtDestination = directArrivalPercent;
                response.Message = $"✅ אין צורך בטעינה: הנסיעה {tripKm:F0} ק\"מ, ותגיעו ליעד עם כ-{directArrivalPercent}% סוללה.";
                return response;
            }

            bool isLowArrivalDirectly = canReachPhysically && directArrivalPercent < MinComfortableArrivalPercent;
            double targetReserveKwh = isLowArrivalDirectly
                ? Math.Max(minReserveKwh, capacityKwh * (MinComfortableArrivalPercent / 100.0))
                : minReserveKwh;

            // ===== צעדים 2–3: סינון וחישוב לכל עמדה פתוחה ופנויה בדרך =====
            var candidates = new List<RankedStationResultDto>();
            foreach (var st in stations)
            {
                if (!PassesBasicFilters(st, pref)) continue;
                if (st.MaxKw <= 0) continue;

                double toStationKm = GeoHelper.RoadDistanceKm(oLat, oLon, st.Latitude, st.Longitude);
                double toDestKm = GeoHelper.RoadDistanceKm(st.Latitude, st.Longitude, dLat, dLon);

                // אילוץ 1: חייבים להגיע לעמדה עם הסוללה הנוכחית
                if (toStationKm > rangeKm) continue;

                // אילוץ 2: העצירה לא מוציאה אותנו יותר מדי מהדרך
                double detourKm = toStationKm + toDestKm - tripKm;
                if (detourKm > RouteRules.MaxDetourKm) continue;

                // כמה קוט"ש יהיו בסוללה כשנגיע לעמדה
                double arrivalAtStationKwh = startKwh - toStationKm * RouteRules.ConsumptionKwhPerKm;

                // כמה צריך לטעון כדי להגיע ליעד ולהשאיר רזרבה בטוחה (לפחות 20% במקרה של סוללה נמוכה)
                double neededKwh = toDestKm * RouteRules.ConsumptionKwhPerKm + targetReserveKwh;
                double chargeKwh = Math.Max(0, neededKwh - arrivalAtStationKwh);

                // אילוץ 3: אם גם סוללה מלאה לא מספיקה מהעמדה ליעד – העמדה לא רלוונטית
                if (arrivalAtStationKwh + chargeKwh > capacityKwh) continue;

                double chargeCost = chargeKwh * st.PricePerKwh;
                double chargeMinutes = chargeKwh / st.MaxKw * 60;
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
                if (isLowArrivalDirectly)
                {
                    response.NoChargeNeeded = true;
                    response.ArrivalPercentAtDestination = Math.Max(0, directArrivalPercent);
                    response.Message =
                        $"⚠️ הסוללה מספיקה לנסיעה ({tripKm:F0} ק\"מ), אך תגיעו ליעד עם כ-{Math.Max(0, directArrivalPercent)}% סוללה בלבד ולא נמצאו תחנות פתוחות ופנויות על המסלול.";
                    return response;
                }

                response.Message = "⚠️ לא נמצאה עמדה פתוחה ופנויה מתאימה בדרך (בטווח הסוללה ועם עקיפה של עד " +
                                   $"{RouteRules.MaxDetourKm:F0} ק\"מ). נסו להרחיב את החיפוש או לטעון לפני היציאה.";
                return response;
            }

            // ===== צעד 4: ציון משוקלל (נרמול min/max בין המועמדות) =====
            double minTime = candidates.Min(c => c.ExtraMinutes!.Value), maxTime = candidates.Max(c => c.ExtraMinutes!.Value);
            double minCost = candidates.Min(c => c.ChargeCost!.Value), maxCost = candidates.Max(c => c.ChargeCost!.Value);

            foreach (var c in candidates)
            {
                double timeScore = 1 - Normalize(c.ExtraMinutes!.Value, minTime, maxTime);
                double costScore = 1 - Normalize(c.ChargeCost!.Value, minCost, maxCost);
                double ratingScore = (c.Station.AverageRating ?? 4.0) / 5.0;

                double score = timeScore * RouteRules.WeightTime +
                               costScore * RouteRules.WeightCost +
                               ratingScore * RouteRules.WeightRating;

                c.MatchPercentage = (int)Math.Clamp(Math.Round(score * 100), 1, 100);
                c.MatchReason = BuildRouteReason(c);
            }

            // ===== צעד 5: מיון מהציון הגבוה לנמוך =====
            response.Stations = MergeSorter.Sort(candidates,
                (a, b) => b.MatchPercentage.CompareTo(a.MatchPercentage));

            if (isLowArrivalDirectly)
            {
                response.NoChargeNeeded = false;
                response.ArrivalPercentAtDestination = Math.Max(0, directArrivalPercent);
                response.Message =
                    $"⚠️ תוכלו להגיע ליעד ישירות, אך תישארו עם כ-{Math.Max(0, directArrivalPercent)}% סוללה בלבד. {response.Stations[0].MatchReason}";
            }
            else
            {
                response.Message = response.Stations[0].MatchReason;
            }

            return response;
        }

        // ---------- פונקציות עזר ----------

        private static double Normalize(double value, double min, double max)
        {
            return max - min < 0.0001 ? 0 : (value - min) / (max - min);
        }

        /// <summary>סינונים בסיסיים: פעילה, פתוחה לפי לו״ז, שקע תואם, פנויה ודירוג מינימלי</summary>
        private static bool PassesBasicFilters(StationDto st, DriverPreferenceRequestDto pref)
        {
            if (!st.IsActive || st.IsClosedBySchedule)
                return false;

            if (!string.IsNullOrWhiteSpace(pref.RequiredChargerType) &&
                !st.ChargerType.Contains(pref.RequiredChargerType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(st.CurrentStatus))
            {
                var status = st.CurrentStatus.Trim();
                if (status.Equals(StationStatuses.Faulty, StringComparison.OrdinalIgnoreCase) ||
                    status.Equals(StationStatuses.Occupied, StringComparison.OrdinalIgnoreCase) ||
                    status.Equals(StationStatuses.Reserved, StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("תקול", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("Fault", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("Broken", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("תפוס", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("משוריינ", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("שמור", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            double rating = st.AverageRating ?? 4.0;
            if (pref.MinRating.HasValue && rating < pref.MinRating.Value)
                return false;

            return true;
        }

        private static string BuildRouteReason(RankedStationResultDto c)
        {
            return $"עצירה מומלצת ב{c.Station.Name}: {c.DetourKm:F1} ק\"מ עקיפה. " +
                   $"מספיק לטעון {c.ChargeKwh:F1} קוט\"ש (~₪{c.ChargeCost:F1}, ~{Math.Ceiling(c.ExtraMinutes ?? 0)} דק' סה\"כ) " +
                   $"ותגיעו ליעד עם כ-{c.ArrivalPercentAtDestination}%. התאמה {c.MatchPercentage}%.";
        }
    }
}