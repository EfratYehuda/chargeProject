using AutoMapper;
using BLL.Exceptions;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UsersVehiclesService : IUsersVehiclesService
    {
        private readonly InterfaceUsersVehicles _vehicleRepo;
        private readonly IMapper _mapper;

        public UsersVehiclesService(InterfaceUsersVehicles vehicleRepo, IMapper mapper)
        {
            _vehicleRepo = vehicleRepo;
            _mapper = mapper;
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            var entities = await _vehicleRepo.GetAllAsync();
            return _mapper.Map<List<VehicleDto>>(entities);
        }

        public async Task<VehicleDto> GetByIdAsync(int id)
        {
            var entity = await _vehicleRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("הרכב לא נמצא");
            return _mapper.Map<VehicleDto>(entity);
        }

        public async Task<VehicleDto> GetOwnedAsync(int id, int driverId)
        {
            var entity = await GetOwnedEntityAsync(id, driverId);
            return _mapper.Map<VehicleDto>(entity);
        }

        public async Task<List<VehicleDto>> GetByDriverIdAsync(int driverId)
        {
            var entities = await _vehicleRepo.GetByDriverIdAsync(driverId);
            return _mapper.Map<List<VehicleDto>>(entities);
        }

        public async Task<VehicleDto> AddAsync(int driverId, CreateUpdateVehicleDto dto)
        {
            if (!await _vehicleRepo.DriverExistsAsync(driverId))
                throw new NotFoundException("פרופיל הנהג לא נמצא");

            var entity = _mapper.Map<UsersVehicles>(dto);
            entity.DriverId = driverId;   // from the token, not from the client

            var savedEntity = await _vehicleRepo.AddAsync(entity);

            var fullEntity = await _vehicleRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<VehicleDto>(fullEntity!);
        }

        // IDOR protection: the vehicle is loaded only if it belongs to this driver.
        public async Task<VehicleDto> UpdateAsync(int id, int driverId, CreateUpdateVehicleDto dto)
        {
            var entity = await GetOwnedEntityAsync(id, driverId);

            _mapper.Map(dto, entity);
            entity.DriverId = driverId;   // the vehicle can never be moved to another driver

            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);
            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        public async Task<VehicleDto> UpdateCurrentBatteryAsync(int id, int driverId, double batteryPercent)
        {
            if (batteryPercent < 0 || batteryPercent > 100)
                throw new BusinessRuleException("אחוז סוללה חייב להיות בין 0 ל-100");

            var entity = await GetOwnedEntityAsync(id, driverId);

            entity.CurrentBattery = batteryPercent;
            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);
            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id, int driverId)
        {
            await GetOwnedEntityAsync(id, driverId);   // throws NotFoundException if it is not his
            await _vehicleRepo.DeleteAsync(id);
        }

        // One place for "give me the vehicle only if it is mine". Not yours and does not exist look the same: 404.
        private async Task<UsersVehicles> GetOwnedEntityAsync(int id, int driverId) =>
            await _vehicleRepo.GetOwnedAsync(id, driverId)
                ?? throw new NotFoundException("הרכב לא נמצא");
    }
}