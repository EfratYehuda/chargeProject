using AutoMapper;
using BLL.Algorithm;
using BLL.Exceptions;
using BLL.Helpers;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ChargingStationsService : IChargingStationsService
    {
        private readonly InterfaceChargingStations _stationsRepo;
        private readonly IMapper _mapper;
        private readonly StationMatchingAlgorithm _algorithm;

        public ChargingStationsService(InterfaceChargingStations stationsRepo, IMapper mapper, StationMatchingAlgorithm algorithm)
        {
            _stationsRepo = stationsRepo;
            _mapper = mapper;
            _algorithm = algorithm;
        }

        public async Task<StationDto> GetByIdAsync(int id)
        {
            var entity = await _stationsRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("העמדה לא נמצאה");
            return _mapper.Map<StationDto>(entity);
        }

        public async Task<List<StationDto>> GetAllAsync()
        {
            var entities = await _stationsRepo.GetAllApprovedAsync();
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetAllForAdminAsync()
        {
            var entities = await _stationsRepo.GetAllAsync();
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetPendingForAdminAsync()
        {
            var entities = await _stationsRepo.GetPendingAsync();
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetByOwnerIdAsync(int ownerId)
        {
            var entities = await _stationsRepo.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType)
        {
            // These values come straight from the query string, so they are checked here.
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                throw new BusinessRuleException("קואורדינטות לא חוקיות");
            if (radiusKm <= 0 || radiusKm > 500)
                throw new BusinessRuleException("הרדיוס חייב להיות בין 0 ל-500 ק\"מ");

            var latDelta = radiusKm / 111.0;
            var lngDelta = radiusKm / (111.0 * Math.Max(Math.Cos(latitude * Math.PI / 180.0), 0.01));

            var minLat = latitude - latDelta;
            var maxLat = latitude + latDelta;
            var minLng = longitude - lngDelta;
            var maxLng = longitude + lngDelta;

            var entities = await _stationsRepo.GetActiveInBoundingBoxAsync(minLat, maxLat, minLng, maxLng, chargerType);
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<StationDto> AddAsync(int ownerId, CreateUpdateStationDto dto)
        {
            if (!await _stationsRepo.OwnerExistsAsync(ownerId))
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

            var entity = _mapper.Map<ChargingStations>(dto);
            entity.OwnerId = ownerId;   // from the token, not from the client
            entity.ApprovalStatus = StationApprovalStatuses.Pending;
            entity.RejectionReason = null;

            var savedEntity = await _stationsRepo.AddAsync(entity);
            return await GetByIdAsync(savedEntity.Id);
        }

        // IDOR protection: the station is loaded only if it belongs to this owner.
        public async Task<StationDto> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto)
        {
            var entity = await GetOwnedEntityAsync(id, ownerId);

            bool locationChanged =
                !string.Equals(entity.Address?.Trim(), dto.Address.Trim(), StringComparison.OrdinalIgnoreCase) ||
                Math.Abs(entity.Latitude - dto.Latitude) > 0.0005 ||
                Math.Abs(entity.Longitude - dto.Longitude) > 0.0005;

            _mapper.Map(dto, entity);
            entity.OwnerId = ownerId;   // the station can never be moved to another owner

            // אם עמדה נדחתה ונערכה מחדש, או שהבעלים שינה מיקום/כתובת — שולחים שוב לאישור מנהל
            if (entity.ApprovalStatus == StationApprovalStatuses.Rejected || locationChanged)
            {
                entity.ApprovalStatus = StationApprovalStatuses.Pending;
                entity.RejectionReason = null;
            }

            await _stationsRepo.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }

        public async Task<StationDto> SetActiveAsync(int id, int ownerId, bool isActive)
        {
            var entity = await GetOwnedEntityAsync(id, ownerId);
            entity.IsActive = isActive;

            await _stationsRepo.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }

        public async Task<StationDto> UpdateBlockedHoursAsync(int id, int ownerId, List<int> blockedHours)
        {
            var entity = await GetOwnedEntityAsync(id, ownerId);
            entity.BlockedHours = StationScheduleHelper.SerializeBlockedHours(blockedHours);

            await _stationsRepo.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }

        public async Task<StationDto> ReviewApprovalAsync(int id, ReviewStationApprovalDto dto)
        {
            var entity = await _stationsRepo.GetByIdForUpdateAsync(id)
                ?? throw new NotFoundException("העמדה לא נמצאה");

            if (dto.ApprovalStatus == StationApprovalStatuses.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new BusinessRuleException("יש לציין סיבת דחייה קצרה עבור בעל העמדה");

            entity.ApprovalStatus = dto.ApprovalStatus;
            entity.RejectionReason = dto.ApprovalStatus == StationApprovalStatuses.Rejected
                ? dto.RejectionReason?.Trim()
                : null;

            await _stationsRepo.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(int id, int ownerId)
        {
            await GetOwnedEntityAsync(id, ownerId);   // throws NotFoundException if it is not his
            await _stationsRepo.DeleteAsync(id);
        }

        private async Task<ChargingStations> GetOwnedEntityAsync(int id, int ownerId) =>
            await _stationsRepo.GetOwnedAsync(id, ownerId)
                ?? throw new NotFoundException("העמדה לא נמצאה");

        public async Task<List<StationDto>> SearchAsync(string? text, int max = 5)
        {
            var entities = await _stationsRepo.SearchAsync(text, max);
            return _mapper.Map<List<StationDto>>(entities);
        }

        // ================== המלצות (אלגוריתם) ==================

        private const double MaxSearchRadiusKm = 500; // אותה מגבלה שנבדקת ב-GetActiveInRadiusAsync
        private const double MinSearchRadiusKm = 30;

        /// <summary>מצב רדיוס: עמדות סביב הנהג, ברדיוס לפי טווח הסוללה</summary>
        public async Task<List<RankedStationResultDto>> RecommendAsync(DriverPreferenceRequestDto request)
        {
            ValidateBatteryInput(request);

            // רדיוס השליפה = טווח הסוללה לפי הרכב (בין 30 ל-500 ק"מ)
            double currentKwh = request.BatteryPercentage / 100.0 * request.BatteryCapacityKwh;
            double rangeKm = currentKwh / RouteRules.ConsumptionKwhPerKm;
            double searchRadius = Math.Clamp(rangeKm, MinSearchRadiusKm, MaxSearchRadiusKm);

            var stations = await GetActiveInRadiusAsync(
                request.DriverLatitude, request.DriverLongitude, searchRadius, request.RequiredChargerType);

            return _algorithm.RankStations(stations, request);
        }

        /// <summary>מצב מסלול: איפה הכי משתלם לעצור בדרך מהמוצא ליעד</summary>
        public async Task<RouteRecommendationResponseDto> RecommendRouteAsync(DriverPreferenceRequestDto request)
        {
            ValidateBatteryInput(request);

            if (!request.HasDestination)
                throw new BusinessRuleException("במצב מסלול חובה לשלוח יעד");

            double dLat = request.DestinationLatitude!.Value, dLon = request.DestinationLongitude!.Value;
            if (dLat < -90 || dLat > 90 || dLon < -180 || dLon > 180)
                throw new BusinessRuleException("קואורדינטות היעד לא חוקיות");

            // רשת השליפה: עיגול סביב נקודת האמצע, שמכסה את כל המסלול + שוליים לעקיפה
            var mid = GeoHelper.Midpoint(request.DriverLatitude, request.DriverLongitude, dLat, dLon);
            double tripKm = GeoHelper.RoadDistanceKm(request.DriverLatitude, request.DriverLongitude, dLat, dLon);
            double searchRadius = Math.Clamp(tripKm / 2 + RouteRules.MaxDetourKm, MinSearchRadiusKm, MaxSearchRadiusKm);

            var stations = await GetActiveInRadiusAsync(mid.Lat, mid.Lon, searchRadius, request.RequiredChargerType);

            return _algorithm.RankRouteStops(stations, request);
        }

        /// <summary>בדיקות קלט משותפות לשני המצבים</summary>
        private static void ValidateBatteryInput(DriverPreferenceRequestDto request)
        {
            if (request.BatteryPercentage < 0 || request.BatteryPercentage > 100)
                throw new BusinessRuleException("אחוז הסוללה חייב להיות בין 0 ל-100");
            if (request.BatteryCapacityKwh <= 0 || request.BatteryCapacityKwh > 200)
                throw new BusinessRuleException("קיבולת הסוללה חייבת להיות בין 0 ל-200 קוט\"ש");
        }
    }
}