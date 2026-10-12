using DTO;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה להזמנות טעינה ושריונים</summary>
    public interface InterfaceChargeBooking
    {
        Task<ChargeBookingDto?> GetByIdAsync(int id);
        Task<List<ChargeBookingDto>> GetAllAsync();
        Task<List<ChargeBookingDto>> GetByDriverIdAsync(int driverId);
        Task<List<ChargeBookingDto>> GetByStationIdAsync(int stationId);
        Task<List<ChargeBookingDto>> GetByOwnerIdAsync(int ownerId);
        /// <summary>יוצר שריון בסטטוס Held (15 דקות)</summary>
        Task<ChargeBookingDto> AddAsync(CreateChargeBookingDto dto);
        Task<ChargeBookingDto?> UpdateAsync(int id, CreateChargeBookingDto dto);
        Task<ChargeBookingDto?> UpdateStatusAsync(int id, string status);
        /// <summary>מסמן שריונים שפג תוקפם כ-Expired ומחזיר כמה עודכנו</summary>
        Task<int> ExpireStaleHoldsAsync();
        Task<bool> DeleteAsync(int id);
    }
}
