using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class ReviewsToChargeRepository : InterfaceReviewsToCharge
    {
        private readonly AppDbContext _context;

        public ReviewsToChargeRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<ReviewsToCharge> ReviewsQuery => _context.Set<ReviewsToCharge>()
            .Include(r => r.Booking).ThenInclude(b => b.Station)
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .AsNoTracking();

        public async Task<ReviewsToCharge?> GetByIdAsync(int id) =>
            await ReviewsQuery.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<List<ReviewsToCharge>> GetAllAsync() =>
            await ReviewsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<List<ReviewsToCharge>> GetByStationIdAsync(int stationId) =>
            await ReviewsQuery.Where(r => r.Booking.StationId == stationId)
                              .OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<List<ReviewsToCharge>> GetByDriverIdAsync(int driverId) =>
            await ReviewsQuery.Where(r => r.DriverId == driverId)
                              .OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<List<ReviewsToCharge>> GetByOwnerIdAsync(int ownerId) =>
            await ReviewsQuery.Where(r => r.Booking.Station.OwnerId == ownerId)
                              .OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<ReviewsToCharge?> GetByBookingIdAsync(int bookingId) =>
            await ReviewsQuery.FirstOrDefaultAsync(r => r.ChargeBookingId == bookingId);

        public async Task<double?> GetStationAverageRatingAsync(int stationId) =>
            await _context.Set<ReviewsToCharge>()
                .Where(r => r.Booking.StationId == stationId)
                .AverageAsync(r => (double?)r.Rating);

        public async Task<ReviewsToCharge> AddAsync(ReviewsToCharge entity)
        {
            _context.Set<ReviewsToCharge>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ReviewsToCharge>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ReviewsToCharge>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- עזרים ---
        public async Task<ChargeBooking?> GetBookingByIdAsync(int bookingId) =>
            await _context.Set<ChargeBooking>().AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId);

        public async Task<bool> ReviewExistsForBookingAsync(int bookingId) =>
            await _context.Set<ReviewsToCharge>().AnyAsync(r => r.ChargeBookingId == bookingId);
    }
}