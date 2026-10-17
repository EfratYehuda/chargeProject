using AutoMapper;
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

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            var entity = await _vehicleRepo.GetByIdAsync(id);
            return _mapper.Map<VehicleDto>(entity);
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            var entities = await _vehicleRepo.GetAllAsync();
            return _mapper.Map<List<VehicleDto>>(entities);
        }

        public async Task<List<VehicleDto>> GetByDriverIdAsync(int driverId)
        {
            var entities = await _vehicleRepo.GetByDriverIdAsync(driverId);
            return _mapper.Map<List<VehicleDto>>(entities);
        }

        public async Task<VehicleDto> AddAsync(CreateUpdateVehicleDto dto)
        {
            if (!await _vehicleRepo.DriverExistsAsync(dto.DriverId))
                throw new KeyNotFoundException($"נהג {dto.DriverId} לא נמצא");

            var entity = _mapper.Map<UsersVehicles>(dto);
            var savedEntity = await _vehicleRepo.AddAsync(entity);

            var fullEntity = await _vehicleRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<VehicleDto>(fullEntity!);
        }

        public async Task<VehicleDto?> UpdateAsync(int id, CreateUpdateVehicleDto dto)
        {
            var entity = await _vehicleRepo.GetByIdAsync(id);
            if (entity == null) return null;

            var originalDriverId = entity.DriverId; // שמירת המזהה המקורי למניעת העברת רכב
            _mapper.Map(dto, entity);
            entity.DriverId = originalDriverId;

            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);
            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        public async Task<VehicleDto?> UpdateCurrentBatteryAsync(int id, double batteryPercent)
        {
            if (batteryPercent < 0 || batteryPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(batteryPercent), "אחוז סוללה חייב להיות בין 0 ל-100");

            var entity = await _vehicleRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.CurrentBattery = batteryPercent;
            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);

            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _vehicleRepo.DeleteAsync(id);
        }
    }
}