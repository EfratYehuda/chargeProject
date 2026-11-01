using DTO.VEHICLESDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChargeBookingService
    {
        Task<ChargeBookingDto?> GetByIdAsync(int id);
        Task<List<ChargeBookingDto>> GetAllAsync();
        Task<List<ChargeBookingDto>> GetByDriverIdAsync(int driverId);
        Task<List<ChargeBookingDto>> GetByStationIdAsync(int stationId);
        Task<List<ChargeBookingDto>> GetByOwnerIdAsync(int ownerId);
        Task<ChargeBookingDto> AddAsync(CreateChargeBookingDto dto);

        // --- מעודכן עם driverId לאבטחת IDOR ---
        Task<ChargeBookingDto?> UpdateAsync(int id, int driverId, CreateChargeBookingDto dto);
        Task<ChargeBookingDto?> UpdateStatusAsync(int id, int driverId, string status);
        Task<bool> DeleteAsync(int id, int driverId);

        Task<int> ExpireStaleHoldsAsync();

        // מתודת עזר לתרגום UserId ל-DriverId
        Task<int?> GetDriverIdByUserIdAsync(int userId);
    }
}