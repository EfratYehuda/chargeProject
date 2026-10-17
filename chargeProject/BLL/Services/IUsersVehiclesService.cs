using DTO.VEHICLESDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUsersVehiclesService
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