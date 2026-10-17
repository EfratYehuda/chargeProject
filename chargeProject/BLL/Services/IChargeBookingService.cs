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
        Task<ChargeBookingDto?> UpdateAsync(int id, CreateChargeBookingDto dto);
        Task<ChargeBookingDto?> UpdateStatusAsync(int id, string status);
        Task<int> ExpireStaleHoldsAsync();
        Task<bool> DeleteAsync(int id);
    }
}