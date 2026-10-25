using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class StationStatusLogsRepository : InterfaceStationStatusLogs
    {
        private readonly AppDbContext _context;

        public StationStatusLogsRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<StationStatusLogs> LogsQuery => _context.Set<StationStatusLogs>()
            .Include(l => l.Station)
            .Include(l => l.CurrentDriver).ThenInclude(d => d.User)
            .AsNoTracking();

        public async Task<StationStatusLogs?> GetByIdAsync(int id) =>
            await LogsQuery.FirstOrDefaultAsync(l => l.Id == id);

        public async Task<List<StationStatusLogs>> GetAllAsync() =>
            await LogsQuery.OrderByDescending(l => l.UpdatedAt).ToListAsync();

        public async Task<List<StationStatusLogs>> GetByStationIdAsync(int stationId) =>
            await LogsQuery.Where(l => l.StationId == stationId)
                           .OrderByDescending(l => l.UpdatedAt)
                           .ToListAsync();

        public async Task<StationStatusLogs?> GetLatestByStationIdAsync(int stationId) =>
            await LogsQuery.Where(l => l.StationId == stationId)
                           .OrderByDescending(l => l.UpdatedAt)
                           .FirstOrDefaultAsync();

        public async Task<List<StationStatusLogs>> GetFaultReportsByOwnerIdAsync(int ownerId, string faultyStatus, int take = 20) =>
            await LogsQuery.Where(l => l.Station.OwnerId == ownerId && l.Status == faultyStatus)
                           .OrderByDescending(l => l.UpdatedAt)
                           .Take(take)
                           .ToListAsync();

        public async Task<StationStatusLogs> AddAsync(StationStatusLogs entity)
        {
            _context.Set<StationStatusLogs>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<StationStatusLogs>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<StationStatusLogs>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- עזרים לוולידציה ---
        public async Task<bool> StationExistsAsync(int stationId) =>
            await _context.Set<ChargingStations>().AnyAsync(s => s.Id == stationId);

        public async Task<bool> DriverExistsAsync(int driverId) =>
            await _context.Set<Drivers>().AnyAsync(d => d.Id == driverId);
    }
}