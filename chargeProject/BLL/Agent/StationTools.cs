using BLL.Algorithm;
using BLL.Services;
using DTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Agent
{
    public class StationTools
    {
        private readonly IChargingStationsService _stationService;
        private readonly StationMatchingAlgorithm _algorithm;
        private readonly LocationTools _locationTools;

        public StationTools(
            IChargingStationsService stationService,
            StationMatchingAlgorithm algorithm,
            LocationTools locationTools)
        {
            _stationService = stationService;
            _algorithm = algorithm;
            _locationTools = locationTools;
        }

        [Description("מאתר ומדרג עד 3 עמדות פתוחות ופנויות לפי קואורדינטות GPS או כתובת טקסטואלית, כולל התאמת סוג שקע ומרחק.")]
        public async Task<List<RankedStationResultDto>> FindBestStationsAsync(
            [Description("קו רוחב של הנהג (אם ידוע)")] double? latitude = null,
            [Description("קו אורך של הנהג (אם ידוע)")] double? longitude = null,
            [Description("כתובת או עיר ורחוב לחיפוש אם אין קואורדינטות, למשל: רוטשילד 50 פתח תקווה")] string? address = null,
            [Description("אחוז סוללה נוכחי (1-100)")] int batteryPercentage = 50,
            [Description("זמן פנוי בדקות לטעינה")] int availableMinutes = 60,
            [Description("סוג שקע מבוקש: CCS (טעינה מהירה DC) או Type2 (AC לשהייה ארוכה). השאר ריק אם לא צוין")] string? chargerType = null,
            [Description("משקל קרבה 0.0-1.0")] double weightDistance = 0.3,
            [Description("משקל מחיר 0.0-1.0")] double weightPrice = 0.25,
            [Description("משקל דירוג 0.0-1.0")] double weightRating = 0.25,
            [Description("משקל מהירות טעינה 0.0-1.0")] double weightSpeed = 0.2)
        {
            double targetLat;
            double targetLng;

            if (!string.IsNullOrWhiteSpace(address))
            {
                using var geoCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var resolved = await _locationTools.GetCoordinatesFromAddressAsync(address, geoCts.Token);

                if (resolved != null)
                {
                    targetLat = resolved.Latitude;
                    targetLng = resolved.Longitude;
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    targetLat = latitude.Value;
                    targetLng = longitude.Value;
                }
                else
                {
                    return new List<RankedStationResultDto>();
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                targetLat = latitude.Value;
                targetLng = longitude.Value;
            }
            else
            {
                return new List<RankedStationResultDto>();
            }

            string? normalizedChargerType = NormalizeChargerType(chargerType);
            var stations = await _stationService.GetActiveInRadiusAsync(
                targetLat,
                targetLng,
                25,
                normalizedChargerType);

            var pref = new DriverPreferenceRequestDto
            {
                DriverLatitude = targetLat,
                DriverLongitude = targetLng,
                BatteryPercentage = Math.Clamp(batteryPercentage, 1, 100),
                AvailableTimeMinutes = Math.Clamp(availableMinutes, 5, 600),
                RequiredChargerType = normalizedChargerType,
                WeightDistance = Math.Clamp(weightDistance, 0.0, 1.0),
                WeightPrice = Math.Clamp(weightPrice, 0.0, 1.0),
                WeightRating = Math.Clamp(weightRating, 0.0, 1.0),
                WeightSpeed = Math.Clamp(weightSpeed, 0.0, 1.0)
            };

            var ranked = _algorithm.RankStations(stations, pref);

            return ranked
                .Where(r => IsBookableNow(r.Station))
                .Take(3)
                .ToList();
        }

        [Description("מחפש עמדות טעינה פעילות במסד הנתונים לפי שם עיר או רחוב (מציג קודם עמדות פתוחות ופנויות).")]
        public async Task<List<StationDto>> SearchStationsAsync(
            [Description("שם העיר או הרחוב, למשל: פתח תקווה")] string? searchText)
        {
            var results = await _stationService.SearchAsync(searchText, 5);
            return results
                .OrderBy(s => IsBookableNow(s) ? 0 : 1)
                .ToList();
        }

        [Description("מחזיר את פרטי העמדה המלאים לפי מזהה מספרי.")]
        public async Task<StationDto?> GetStationDetailsAsync(
            [Description("מזהה העמדה (Id)")] int stationId)
        {
            return await _stationService.GetByIdAsync(stationId);
        }

        private static string? NormalizeChargerType(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (raw.Contains("CCS", StringComparison.OrdinalIgnoreCase) ||
                raw.Contains("DC", StringComparison.OrdinalIgnoreCase) ||
                raw.Contains("מהיר", StringComparison.OrdinalIgnoreCase))
            {
                return ChargerTypes.CCS;
            }

            if (raw.Contains("Type2", StringComparison.OrdinalIgnoreCase) ||
                raw.Contains("Type 2", StringComparison.OrdinalIgnoreCase) ||
                raw.Contains("AC", StringComparison.OrdinalIgnoreCase))
            {
                return ChargerTypes.Type2;
            }

            return null;
        }

        private static bool IsBookableNow(StationDto st)
        {
            if (!st.IsActive || st.IsClosedBySchedule)
                return false;

            if (string.IsNullOrWhiteSpace(st.CurrentStatus))
                return true;

            var status = st.CurrentStatus.Trim();
            if (status.Equals(StationStatuses.Occupied, StringComparison.OrdinalIgnoreCase) ||
                status.Equals(StationStatuses.Reserved, StringComparison.OrdinalIgnoreCase) ||
                status.Equals(StationStatuses.Faulty, StringComparison.OrdinalIgnoreCase) ||
                status.Contains("תקול", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Fault", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Broken", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("תפוס", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("משוריינ", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("שמור", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}