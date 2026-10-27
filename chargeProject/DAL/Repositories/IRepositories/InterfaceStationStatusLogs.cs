using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceStationStatusLogs
    {
        Task<StationStatusLogs?> GetByIdAsync(int id);
        Task<List<StationStatusLogs>> GetAllAsync();
        Task<List<StationStatusLogs>> GetByStationIdAsync(int stationId);
        Task<StationStatusLogs?> GetLatestByStationIdAsync(int stationId);
        Task<List<StationStatusLogs>> GetFaultReportsByOwnerIdAsync(int ownerId, string faultyStatus, int take = 20);
        Task<StationStatusLogs> AddAsync(StationStatusLogs entity);
        Task<bool> DeleteAsync(int id);

        // פונקציות עזר לוולידציה (הועברו לכאן)
        Task<bool> StationExistsAsync(int stationId);
        Task<bool> DriverExistsAsync(int driverId);
    }
}