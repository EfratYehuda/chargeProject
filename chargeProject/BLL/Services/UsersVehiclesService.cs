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

        // --- אבטחת IDOR: עדכון רכב רק אם הוא שייך לנהג ---
        public async Task<VehicleDto?> UpdateAsync(int id, int driverId, CreateUpdateVehicleDto dto)
        {
            var entity = await _vehicleRepo.GetOwnedAsync(id, driverId);
            if (entity == null) return null;

            var originalDriverId = entity.DriverId; // שמירת המזהה המקורי למניעת העברת רכב
            _mapper.Map(dto, entity);
            entity.DriverId = originalDriverId;

            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);
            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        // --- אבטחת IDOR: עדכון סוללה רק אם הרכב שייך לנהג ---
        public async Task<VehicleDto?> UpdateCurrentBatteryAsync(int id, int driverId, double batteryPercent)
        {
            if (batteryPercent < 0 || batteryPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(batteryPercent), "אחוז סוללה חייב להיות בין 0 ל-100");

            var entity = await _vehicleRepo.GetOwnedAsync(id, driverId);
            if (entity == null) return null;

            entity.CurrentBattery = batteryPercent;
            var updatedEntity = await _vehicleRepo.UpdateAsync(entity);

            return _mapper.Map<VehicleDto>(updatedEntity);
        }

        // --- אבטחת IDOR: מחיקת רכב רק אם הוא שייך לנהג ---
        public async Task<bool> DeleteAsync(int id, int driverId)
        {
            var entity = await _vehicleRepo.GetOwnedAsync(id, driverId);
            if (entity == null) return false;

            return await _vehicleRepo.DeleteAsync(id);
        }

        // מתודת עזר הממירה את ה-UserId ל-DriverId דרך ה-Repository
        public async Task<int?> GetDriverIdByUserIdAsync(int userId)
        {
            return await _vehicleRepo.GetDriverIdByUserIdAsync(userId);
        }
    }
}