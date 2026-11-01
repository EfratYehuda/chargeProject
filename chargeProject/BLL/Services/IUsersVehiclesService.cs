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

        // מעודכן עם driverId לאבטחת IDOR:
        Task<VehicleDto?> UpdateAsync(int id, int driverId, CreateUpdateVehicleDto dto);
        Task<VehicleDto?> UpdateCurrentBatteryAsync(int id, int driverId, double batteryPercent);
        Task<bool> DeleteAsync(int id, int driverId);

        // מתודת עזר לחיפוש ה-DriverId לפי ה-UserId מהטוקן:
        Task<int?> GetDriverIdByUserIdAsync(int userId);
    }
}