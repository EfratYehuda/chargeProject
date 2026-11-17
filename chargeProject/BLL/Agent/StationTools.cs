using BLL.Algorithm;
using BLL.Services;
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

        [Description("מאתר ומדרג את 3 העמדות הטובות ביותר לפי קואורדינטות GPS או כתובת טקסטואלית, כולל חישוב מרחק אווירי וציון התאמה.")]
        public async Task<List<RankedStationResult>> FindBestStationsAsync(
            [Description("קו רוחב של הנהג (אם ידוע)")] double? latitude = null,
            [Description("קו אורך של הנהג (אם ידוע)")] double? longitude = null,
            [Description("כתובת או עיר ורחוב לחיפוש אם אין קואורדינטות, למשל: רוטשילד פתח תקווה")] string? address = null,
            [Description("אחוז סוללה נוכחי (0-100)")] int batteryPercentage = 50,
            [Description("זמן פנוי בדקות לטעינה")] int availableMinutes = 60,
            [Description("משקל קרבה 0.0-1.0")] double weightDistance = 0.3,
            [Description("משקל מחיר 0.0-1.0")] double weightPrice = 0.3,
            [Description("משקל דירוג 0.0-1.0")] double weightRating = 0.4)
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
                    return new List<RankedStationResult>();
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                targetLat = latitude.Value;
                targetLng = longitude.Value;
            }
            else
            {
                return new List<RankedStationResult>();
            }

            var stations = await _stationService.GetActiveInRadiusAsync(targetLat, targetLng, 25, null);

            var pref = new DriverPreferenceRequest
            {
                DriverLatitude = targetLat,
                DriverLongitude = targetLng,
                BatteryPercentage = batteryPercentage,
                AvailableTimeMinutes = availableMinutes,
                WeightDistance = weightDistance,
                WeightPrice = weightPrice,
                WeightRating = weightRating
            };

            return _algorithm.RankStations(stations, pref).Take(3).ToList();
        }

        [Description("מחפש עמדות טעינה פעילות במסד הנתונים לפי שם עיר או רחוב.")]
        public async Task<List<StationDto>> SearchStationsAsync(
            [Description("שם העיר או הרחוב, למשל: פתח תקווה")] string? searchText)
        {
            return await _stationService.SearchAsync(searchText, 5);
        }

        [Description("מחזיר את פרטי העמדה המלאים לפי מזהה מספרי.")]
        public async Task<StationDto?> GetStationDetailsAsync(
            [Description("מזהה העמדה (Id)")] int stationId)
        {
            return await _stationService.GetByIdAsync(stationId);
        }
    }
}