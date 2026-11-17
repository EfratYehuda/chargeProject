using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Algorithm
{
    public class StationMatchingAlgorithm
    {
        public List<RankedStationResultDto> RankStations(
            List<StationDto> stations,
            DriverPreferenceRequestDto pref)
        {
            var results = new List<RankedStationResultDto>();

            // הערכת טווח נסיעה נותר: כ-3 ק"מ לכל 1% סוללה
            double estimatedRangeKm = Math.Max(pref.BatteryPercentage * 3.0, 1.0);
            bool isEmergency = pref.BatteryPercentage <= 10; // האם במצב סוללה קריטית?

            foreach (var st in stations)
            {
                // 1. חישוב מרחק מדויק מהנהג לעמדה בק"מ
                double dist = GeoHelper.AirDistanceKm(pref.DriverLatitude, pref.DriverLongitude, st.Latitude, st.Longitude);

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

                results.Add(new RankedStationResultDto
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
                    .Select(s => new { Station = s, Dist = GeoHelper.AirDistanceKm(pref.DriverLatitude, pref.DriverLongitude, s.Latitude, s.Longitude) })
                    .OrderBy(x => x.Dist)
                    .First();

                results.Add(new RankedStationResultDto
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
    }
}