using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
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

        public ChargingStationsService(InterfaceChargingStations stationsRepo, IMapper mapper)
        {
            _stationsRepo = stationsRepo;
            _mapper = mapper;
        }

        public async Task<StationDto?> GetByIdAsync(int id)
        {
            var entity = await _stationsRepo.GetByIdAsync(id);
            return _mapper.Map<StationDto>(entity);
        }

        public async Task<List<StationDto>> GetAllAsync()
        {
            var entities = await _stationsRepo.GetAllAsync();
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetByOwnerIdAsync(int ownerId)
        {
            var entities = await _stationsRepo.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType)
        {
            var latDelta = radiusKm / 111.0;
            var lngDelta = radiusKm / (111.0 * Math.Max(Math.Cos(latitude * Math.PI / 180.0), 0.01));

            var minLat = latitude - latDelta;
            var maxLat = latitude + latDelta;
            var minLng = longitude - lngDelta;
            var maxLng = longitude + lngDelta;

            var entities = await _stationsRepo.GetActiveInBoundingBoxAsync(minLat, maxLat, minLng, maxLng, chargerType);
            return _mapper.Map<List<StationDto>>(entities);
        }

        public async Task<StationDto> AddAsync(CreateUpdateStationDto dto)
        {
            if (!await _stationsRepo.OwnerExistsAsync(dto.OwnerId))
                throw new KeyNotFoundException($"בעל עמדות {dto.OwnerId} לא נמצא");

            var entity = _mapper.Map<ChargingStations>(dto);

            var savedEntity = await _stationsRepo.AddAsync(entity);
            return await GetByIdAsync(savedEntity.Id) ?? _mapper.Map<StationDto>(savedEntity);
        }

        // --- אבטחת IDOR: עדכון עמדה רק אם היא שייכת לבעלים ---
        public async Task<StationDto?> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto)
        {
            // מחפשים את העמדה רק אם ה-Id וה-OwnerId תואמים בדיוק
            var entity = await _stationsRepo.GetOwnedAsync(id, ownerId);
            if (entity == null) return null; // לא נמצאה או לא שייכת למשתמש

            _mapper.Map(dto, entity);
            entity.OwnerId = ownerId; // מוודאים שהבעלים נשאר נכון

            var updatedEntity = await _stationsRepo.UpdateAsync(entity);
            return await GetByIdAsync(id) ?? _mapper.Map<StationDto>(updatedEntity);
        }

        // --- אבטחת IDOR: שינוי סטטוס פעילות רק לבעלים של העמדה ---
        public async Task<StationDto?> SetActiveAsync(int id, int ownerId, bool isActive)
        {
            var entity = await _stationsRepo.GetOwnedAsync(id, ownerId);
            if (entity == null) return null;

            entity.IsActive = isActive;
            var updatedEntity = await _stationsRepo.UpdateAsync(entity);

            return await GetByIdAsync(id) ?? _mapper.Map<StationDto>(updatedEntity);
        }

        // --- אבטחת IDOR: מחיקת עמדה רק אם היא שייכת לבעלים ---
        public async Task<bool> DeleteAsync(int id, int ownerId)
        {
            var entity = await _stationsRepo.GetOwnedAsync(id, ownerId);
            if (entity == null) return false;

            return await _stationsRepo.DeleteAsync(id);
        }

        public async Task<int?> GetOwnerIdByUserIdAsync(int userId)
        {
            return await _stationsRepo.GetOwnerIdByUserIdAsync(userId);
        }
    }
}