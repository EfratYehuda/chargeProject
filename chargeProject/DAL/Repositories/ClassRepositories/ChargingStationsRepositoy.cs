using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class ChargingStationsRepository : InterfaceChargingStations
    {
        private readonly AppDbContext _context;

        public ChargingStationsRepository(AppDbContext context)
        {
            _context = context;
        }

        // צירפתי כאן את ההזמנות (לחישוב דירוג) והלוגים (לסטטוס) כפי שמוגדר במאפר
        private IQueryable<ChargingStations> Stations => _context.Set<ChargingStations>()
            .Include(s => s.Bookings).ThenInclude(b => b.Review)
            .Include(s => s.Logs)
            .AsNoTracking();

        public async Task<ChargingStations?> GetByIdAsync(int id) =>
            await Stations.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<ChargingStations>> GetAllAsync() =>
            await Stations.ToListAsync();

        public async Task<List<ChargingStations>> GetByOwnerIdAsync(int ownerId) =>
            await Stations.Where(s => s.OwnerId == ownerId).ToListAsync();

        public async Task<List<ChargingStations>> GetActiveInBoundingBoxAsync(double minLat, double maxLat, double minLng, double maxLng, string? chargerType)
        {
            var query = Stations.Where(s => s.IsActive
                && s.Latitude >= minLat && s.Latitude <= maxLat
                && s.Longitude >= minLng && s.Longitude <= maxLng);

            if (!string.IsNullOrWhiteSpace(chargerType))
                query = query.Where(s => s.ChargerType == chargerType);

            return await query.ToListAsync();
        }

        public async Task<ChargingStations> AddAsync(ChargingStations entity)
        {
            _context.Set<ChargingStations>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ChargingStations?> UpdateAsync(ChargingStations entity)
        {
            _context.Set<ChargingStations>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ChargingStations>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ChargingStations>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> OwnerExistsAsync(int ownerId)
        {
            return await _context.Set<Owner>().AnyAsync(o => o.Id == ownerId);
        }
    }
}