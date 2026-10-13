using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.IRepositories;
using DAL;
using DAL.Classes;
using DAL.Data;
using DTO;
using DTO.REVIEWDTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה לביקורות על טעינות</summary>
    public class ReviewsToChargeRepository : InterfaceReviewsToCharge
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReviewsToChargeRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<ReviewsToCharge> Reviews => _context.Set<ReviewsToCharge>().AsNoTracking();

        public async Task<ReviewDto?> GetByIdAsync(int id) =>
            await Reviews.Where(r => r.Id == id)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<ReviewDto>> GetAllAsync() =>
            await Reviews.OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ReviewDto>> GetByStationIdAsync(int stationId) =>
            await Reviews.Where(r => r.Booking.StationId == stationId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ReviewDto>> GetByDriverIdAsync(int driverId) =>
            await Reviews.Where(r => r.DriverId == driverId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ReviewDto>> GetByOwnerIdAsync(int ownerId) =>
            await Reviews.Where(r => r.Booking.Station.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<ReviewDto?> GetByBookingIdAsync(int bookingId) =>
            await Reviews.Where(r => r.ChargeBookingId == bookingId)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<double?> GetStationAverageRatingAsync(int stationId) =>
            await Reviews.Where(r => r.Booking.StationId == stationId)
                .AverageAsync(r => (double?)r.Rating);

        public async Task<ReviewDto> AddAsync(CreateReviewDto dto)
        {
            var booking = await _context.Set<ChargeBooking>().AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == dto.ChargeBookingId)
                ?? throw new KeyNotFoundException($"הזמנה {dto.ChargeBookingId} לא נמצאה");

            if (booking.DriverId != dto.DriverId)
                throw new InvalidOperationException("ההזמנה אינה שייכת לנהג");
            if (booking.Status != BookingStatuses.Completed)
                throw new InvalidOperationException("ניתן לדרג רק טעינה שהסתיימה");
            if (await _context.Set<ReviewsToCharge>().AnyAsync(r => r.ChargeBookingId == dto.ChargeBookingId))
                throw new InvalidOperationException("כבר נכתבה ביקורת להזמנה זו");

            var entity = _mapper.Map<ReviewsToCharge>(dto);
            entity.CreatedAt = DateTime.Now;

            _context.Set<ReviewsToCharge>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ReviewsToCharge>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ReviewsToCharge>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
