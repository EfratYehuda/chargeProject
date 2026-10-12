using DTO;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה לרכבים של נהגים</summary>
    public interface InterfaceUsersVehicles
    {
        Task<VehicleDto?> GetByIdAsync(int id);
        Task<List<VehicleDto>> GetAllAsync();
        Task<List<VehicleDto>> GetByDriverIdAsync(int driverId);
        Task<VehicleDto> AddAsync(CreateUpdateVehicleDto dto);
        Task<VehicleDto?> UpdateAsync(int id, CreateUpdateVehicleDto dto);
        Task<VehicleDto?> UpdateCurrentBatteryAsync(int id, double batteryPercent);
        Task<bool> DeleteAsync(int id);
    }
}
