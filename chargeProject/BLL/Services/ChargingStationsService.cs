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
            // החישוב המתמטי עבר לסרוויס, היכן שהלוגיקה העסקית אמורה להיות
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

        public async Task<StationDto?> UpdateAsync(int id, CreateUpdateStationDto dto)
        {
            var entity = await _stationsRepo.GetByIdAsync(id);
            if (entity == null) return null;

            var originalOwnerId = entity.OwnerId;
            _mapper.Map(dto, entity);
            entity.OwnerId = originalOwnerId; // שומרים על הבעלים המקורי בעדכון

            var updatedEntity = await _stationsRepo.UpdateAsync(entity);
            return await GetByIdAsync(id) ?? _mapper.Map<StationDto>(updatedEntity);
        }

        public async Task<StationDto?> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _stationsRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsActive = isActive;
            var updatedEntity = await _stationsRepo.UpdateAsync(entity);

            return await GetByIdAsync(id) ?? _mapper.Map<StationDto>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _stationsRepo.DeleteAsync(id);
        }
    }
}