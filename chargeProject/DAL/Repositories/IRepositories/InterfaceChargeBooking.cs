using DAL.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceChargeBooking
    {
        Task<ChargeBooking?> GetByIdAsync(int id);
        Task<List<ChargeBooking>> GetAllAsync();
        Task<List<ChargeBooking>> GetByDriverIdAsync(int driverId);
        Task<List<ChargeBooking>> GetByStationIdAsync(int stationId);
        Task<List<ChargeBooking>> GetByOwnerIdAsync(int ownerId);
        Task<ChargeBooking> AddAsync(ChargeBooking entity);
        Task<ChargeBooking?> UpdateAsync(ChargeBooking entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> HasActiveBookingAsync(int vehicleId, string[] activeStatuses);
        Task<List<ChargeBooking>> GetStaleHoldsAsync(DateTime threshold);
        Task SaveChangesAsync();
        
        // עזר לבדיקות
        Task<ChargingStations?> GetStationByIdAsync(int stationId);
        Task<UsersVehicles?> GetVehicleByIdAsync(int vehicleId);
    }
}