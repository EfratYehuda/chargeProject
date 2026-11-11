using AutoMapper;
using BLL.Exceptions;
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

        public async Task<StationDto> GetByIdAsync(int id)
        {
            var entity = await _stationsRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("העמדה לא נמצאה");
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

            var savedEntity = await _stationsRepo.AddAsync(entity);
            return await GetByIdAsync(savedEntity.Id);
        }

        // IDOR protection: the station is loaded only if it belongs to this owner.
        public async Task<StationDto> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto)
        {
            var entity = await GetOwnedEntityAsync(id, ownerId);

            _mapper.Map(dto, entity);
            entity.OwnerId = ownerId;   // the station can never be moved to another owner

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

        public async Task DeleteAsync(int id, int ownerId)
        {
            await GetOwnedEntityAsync(id, ownerId);   // throws NotFoundException if it is not his
            await _stationsRepo.DeleteAsync(id);
        }

        private async Task<ChargingStations> GetOwnedEntityAsync(int id, int ownerId) =>
            await _stationsRepo.GetOwnedAsync(id, ownerId)
                ?? throw new NotFoundException("העמדה לא נמצאה");
    }
}