using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTO; // לצורך הגישה ל-StationStatuses. אם הסטטוסים מוגדרים במקום אחר, עדכני את ה-using

namespace DAL.Repositories.ClassRepositories
{
    public class OwnerRepository : InterfaceOwner
    {
        private readonly AppDbContext _context;

        public OwnerRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Owner> OwnersQuery => _context.Set<Owner>()
            .Include(o => o.User)
            .Include(o => o.Stations)
            .AsNoTracking();

        public async Task<Owner?> GetByIdAsync(int id) =>
            await OwnersQuery.FirstOrDefaultAsync(o => o.Id == id);

        public async Task<List<Owner>> GetAllAsync() =>
            await OwnersQuery.ToListAsync();

        public async Task<Owner?> GetByUserIdAsync(int userId) =>
            await OwnersQuery.FirstOrDefaultAsync(o => o.UserId == userId);

        public async Task<Owner> AddAsync(Owner entity)
        {
            _context.Set<Owner>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Owner>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<Owner>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int ownerId) =>
            await _context.Set<Owner>().AnyAsync(o => o.Id == ownerId);

        public async Task<bool> UserExistsAsync(int userId) =>
            await _context.Set<Users>().AnyAsync(u => u.Id == userId);

        public async Task<bool> OwnerExistsForUserAsync(int userId) =>
            await _context.Set<Owner>().AnyAsync(o => o.UserId == userId);

        // --- פונקציות שליפה ייעודיות לדאשבורד ---

        public async Task<List<ChargingStations>> GetStationsByOwnerIdAsync(int ownerId) =>
            await _context.Set<ChargingStations>()
                .Include(s => s.Bookings).ThenInclude(b => b.Review) // לחישוב ממוצע בעמדה
                .Include(s => s.Logs) // לחישוב סטטוס
                .Where(s => s.OwnerId == ownerId)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<ReviewsToCharge>> GetRecentReviewsByOwnerIdAsync(int ownerId, int count) =>
            await _context.Set<ReviewsToCharge>()
                .Include(r => r.Booking).ThenInclude(b => b.Station)
                .Include(r => r.Driver).ThenInclude(d => d.User)
                .Where(r => r.Booking.Station.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<StationStatusLogs>> GetRecentFaultsByOwnerIdAsync(int ownerId, int count, string faultyStatus) =>
            await _context.Set<StationStatusLogs>()
                .Include(l => l.Station)
                .Include(l => l.CurrentDriver).ThenInclude(d => d.User)
                .Where(l => l.Station.OwnerId == ownerId && l.Status == faultyStatus)
                .OrderByDescending(l => l.UpdatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

        public async Task<int> GetTotalBookingsByOwnerIdAsync(int ownerId) =>
            await _context.Set<ChargeBooking>()
                .CountAsync(b => b.Station.OwnerId == ownerId);

        public async Task<double?> GetAverageRatingByOwnerIdAsync(int ownerId) =>
            await _context.Set<ReviewsToCharge>()
                .Where(r => r.Booking.Station.OwnerId == ownerId)
                .AverageAsync(r => (double?)r.Rating);
    }
}