using DTO.VEHICLESDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChargeBookingService
    {
        // Reads
        Task<List<ChargeBookingDto>> GetAllAsync();                          // admin only (enforced in the controller)
        Task<ChargeBookingDto> GetByIdAsync(int id);                         // admin only (enforced in the controller)
        Task<ChargeBookingDto> GetOwnedAsync(int id, int driverId);          // a driver reading HIS booking
        Task<List<ChargeBookingDto>> GetByDriverIdAsync(int driverId);
        Task<List<ChargeBookingDto>> GetByStationIdAsync(int stationId);
        Task<List<ChargeBookingDto>> GetByOwnerIdAsync(int ownerId);

        // Writes. driverId always comes from the token, never from the request body.
        Task<ChargeBookingDto> AddAsync(int driverId, CreateChargeBookingDto dto);
        Task<ChargeBookingDto> UpdateAsync(int id, int driverId, CreateChargeBookingDto dto);
        Task<ChargeBookingDto> UpdateStatusAsync(int id, int driverId, string status);
        Task DeleteAsync(int id, int driverId);

        Task<int> ExpireStaleHoldsAsync();
    }
}