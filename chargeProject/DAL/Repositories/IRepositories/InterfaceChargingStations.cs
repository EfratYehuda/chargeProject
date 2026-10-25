using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceChargingStations
    {
        Task<ChargingStations?> GetByIdAsync(int id);
        Task<List<ChargingStations>> GetAllAsync();
        Task<List<ChargingStations>> GetByOwnerIdAsync(int ownerId);
        // שליפה מרובעת פשוטה ב-DB
        Task<List<ChargingStations>> GetActiveInBoundingBoxAsync(double minLat, double maxLat, double minLng, double maxLng, string? chargerType);
        Task<ChargingStations> AddAsync(ChargingStations entity);
        Task<ChargingStations?> UpdateAsync(ChargingStations entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> OwnerExistsAsync(int ownerId);
    }
}