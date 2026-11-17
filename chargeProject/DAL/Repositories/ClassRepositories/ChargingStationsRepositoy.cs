using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using DTO;
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

        // טעינת הבעלים (לאימות מנהל), ההזמנות (לחישוב דירוג) והלוגים לסטטוס
        private IQueryable<ChargingStations> Stations => _context.Set<ChargingStations>()
            .Include(s => s.Owner).ThenInclude(o => o.User)
            .Include(s => s.Bookings).ThenInclude(b => b.Review)
            .Include(s => s.Logs)
            .AsNoTracking();

        public async Task<ChargingStations>? GetOwnerAsync(int id, int userId) =>
            await Stations.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == userId);

        public async Task<ChargingStations?> GetByIdAsync(int id) =>
            await Stations.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<ChargingStations?> GetByIdForUpdateAsync(int id) =>
            await _context.Set<ChargingStations>().FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<ChargingStations>> GetAllAsync() =>
            await Stations
                .OrderBy(s => s.ApprovalStatus == StationApprovalStatuses.Pending ? 0 : 1)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

        public async Task<List<ChargingStations>> GetAllApprovedAsync() =>
            await Stations
                .Where(s => s.ApprovalStatus == StationApprovalStatuses.Approved)
                .ToListAsync();

        public async Task<List<ChargingStations>> GetPendingAsync() =>
            await Stations
                .Where(s => s.ApprovalStatus == StationApprovalStatuses.Pending)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

        public async Task<List<ChargingStations>> GetByOwnerIdAsync(int ownerId) =>
            await Stations.Where(s => s.OwnerId == ownerId).ToListAsync();

        public async Task<ChargingStations?> GetOwnedAsync(int id, int ownerId) =>
            await _context.Set<ChargingStations>()
                .Include(s => s.Owner).ThenInclude(o => o.User)
                .Include(s => s.Bookings).ThenInclude(b => b.Review)
                .Include(s => s.Logs)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

        public async Task<List<ChargingStations>> GetActiveInBoundingBoxAsync(double minLat, double maxLat, double minLng, double maxLng, string? chargerType)
        {
            var query = Stations.Where(s => s.IsActive
                && s.ApprovalStatus == StationApprovalStatuses.Approved
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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

        public async Task<int?> GetOwnerIdByUserIdAsync(int userId)
        {
            var owner = await _context.Set<Owner>().FirstOrDefaultAsync(o => o.UserId == userId);
            return owner?.Id;
        }

        public async Task<List<ChargingStations>> SearchAsync(string? text, int max = 5)
        {
            var query = Stations.Where(s => s.IsActive && s.ApprovalStatus == StationApprovalStatuses.Approved);
            if (!string.IsNullOrWhiteSpace(text))
            {
                query = query.Where(s => s.Address.Contains(text) || s.Name.Contains(text));
            }
            return await query.Take(max).ToListAsync();
        }
    }
}