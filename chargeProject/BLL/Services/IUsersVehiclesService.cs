using DTO.VEHICLESDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUsersVehiclesService
    {
        // Reads. Everything that "returns one item" throws NotFoundException instead of returning null.
        Task<List<VehicleDto>> GetAllAsync();                           // admin only (enforced in the controller)
        Task<VehicleDto> GetByIdAsync(int id);                          // admin only (enforced in the controller)
        Task<VehicleDto> GetOwnedAsync(int id, int driverId);           // a driver reading HIS vehicle
        Task<List<VehicleDto>> GetByDriverIdAsync(int driverId);

        // Writes. driverId always comes from the token, never from the request body.
        Task<VehicleDto> AddAsync(int driverId, CreateUpdateVehicleDto dto);
        Task<VehicleDto> UpdateAsync(int id, int driverId, CreateUpdateVehicleDto dto);
        Task<VehicleDto> UpdateCurrentBatteryAsync(int id, int driverId, double batteryPercent);
        Task DeleteAsync(int id, int driverId);
    }
}