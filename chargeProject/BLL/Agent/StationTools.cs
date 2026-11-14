using BLL.Algorithm;
using BLL.Services;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BLL.Agent
{
    public class StationTools
    {
        private readonly IChargingStationsService _stationService;
        private readonly StationMatchingAlgorithm _algorithm;

        public StationTools(
            IChargingStationsService stationService,
            StationMatchingAlgorithm algorithm)
        {
            _stationService = stationService;
            _algorithm = algorithm;
        }

        [Description("מפעיל את אלגוריתם השקלול החכם לאיתור העמדות המותאמות ביותר לפי מיקום הנהג, אחוז סוללה, זמן פנוי והעדפות.")]
        public async Task<List<RankedStationResult>> FindBestStationsAsync(
            [Description("קו רוחב (Latitude) של הנהג")] double latitude,
            [Description("קו אורך (Longitude) של הנהג")] double longitude,
            [Description("אחוז סוללה נוכחי של הנהג (0 עד 100)")] int batteryPercentage = 50,
            [Description("זמן פנוי בדקות לטעינה (למשל: 120 לשעתיים פגישה)")] int availableMinutes = 60,
            [Description("משקל הקרבה מ-0.0 עד 1.0 (למשל: 0.5 אם הנהג הדגיש שחשוב לו קרוב)")] double weightDistance = 0.3,
            [Description("משקל המחיר מ-0.0 עד 1.0 (למשל: 0.5 אם הנהג הדגיש שחשוב לו זול)")] double weightPrice = 0.3,
            [Description("משקל הדירוג מ-0.0 עד 1.0 (למשל: 0.5 אם הנהג הדגיש שחשוב לו עמדה בדירוג גבוה)")] double weightRating = 0.4)
        {
            // 1. שולף עמדות פעילות ברדיוס 25 ק"מ ממיקום הנהג
            var stations = await _stationService.GetActiveInRadiusAsync(latitude, longitude, 25, null);

            // 2. בונה את אובייקט ההעדפות
            var pref = new DriverPreferenceRequest
            {
                DriverLatitude = latitude,
                DriverLongitude = longitude,
                BatteryPercentage = batteryPercentage,
                AvailableTimeMinutes = availableMinutes,
                WeightDistance = weightDistance,
                WeightPrice = weightPrice,
                WeightRating = weightRating
            };

            // 3. מריץ את האלגוריתם החכם שלך ומחזיר את 3 העמדות המנצחות!
            return _algorithm.RankStations(stations, pref).Take(3).ToList();
        }

        [Description("מחפש עמדות טעינה פעילות במסד הנתונים לפי שם עיר או רחוב.")]
        public async Task<List<StationDto>> SearchStationsAsync(
            [Description("שם העיר או הרחוב לחיפוש, למשל: פתח תקווה, רוטשילד")] string? searchText)
        {
            return await _stationService.SearchAsync(searchText, 5);
        }

        [Description("מחזיר את כל פרטי העמדה המלאים לפי מזהה (Id).")]
        public async Task<StationDto?> GetStationDetailsAsync(
            [Description("מזהה העמדה (Id) במסד הנתונים")] int stationId)
        {
            return await _stationService.GetByIdAsync(stationId);
        }
    }
}
