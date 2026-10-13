using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.IRepositories;
using DAL;
using DAL.Classes;
using DAL.Data;
using DTO;
using DTO.REVIEWDTO;
using DTO.STATIONDTO;
using DTO.USERDTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה לנתוני בעלי עמדות ולוח הבקרה שלהם</summary>
    public class OwnerRepository : InterfaceOwner
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OwnerRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Owner> Owners => _context.Set<Owner>().AsNoTracking();

        public async Task<OwnerDto?> GetByIdAsync(int id) =>
            await Owners.Where(o => o.Id == id)
                .ProjectTo<OwnerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<OwnerDto>> GetAllAsync() =>
            await Owners.ProjectTo<OwnerDto>(_mapper.ConfigurationProvider).ToListAsync();

        public async Task<OwnerDto?> GetByUserIdAsync(int userId) =>
            await Owners.Where(o => o.UserId == userId)
                .ProjectTo<OwnerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<OwnerDto> AddAsync(CreateOwnerDto dto)
        {
            if (!await _context.Set<Users>().AnyAsync(u => u.Id == dto.UserId))
                throw new KeyNotFoundException($"משתמש {dto.UserId} לא נמצא");
            if (await _context.Set<Owner>().AnyAsync(o => o.UserId == dto.UserId))
                throw new InvalidOperationException("למשתמש כבר קיים פרופיל בעל עמדות");

            var entity = _mapper.Map<Owner>(dto);
            _context.Set<Owner>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Owner>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<Owner>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OwnerDashboardDto?> GetDashboardAsync(int ownerId)
        {
            if (!await Owners.AnyAsync(o => o.Id == ownerId)) return null;

            var cfg = _mapper.ConfigurationProvider;

            var stations = await _context.Set<ChargingStations>().AsNoTracking()
                .Where(s => s.OwnerId == ownerId)
                .ProjectTo<StationDto>(cfg)
                .ToListAsync();

            var recentReviews = await _context.Set<ReviewsToCharge>().AsNoTracking()
                .Where(r => r.Booking.Station.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ProjectTo<ReviewDto>(cfg)
                .ToListAsync();

            var recentFaults = await _context.Set<StationStatusLogs>().AsNoTracking()
                .Where(l => l.Station.OwnerId == ownerId && l.Status == StationStatuses.Faulty)
                .OrderByDescending(l => l.UpdatedAt)
                .Take(10)
                .ProjectTo<StationStatusLogDto>(cfg)
                .ToListAsync();

            var totalBookings = await _context.Set<ChargeBooking>()
                .CountAsync(b => b.Station.OwnerId == ownerId);

            var avgRating = await _context.Set<ReviewsToCharge>()
                .Where(r => r.Booking.Station.OwnerId == ownerId)
                .AverageAsync(r => (double?)r.Rating);

            return new OwnerDashboardDto
            {
                OwnerId = ownerId,
                TotalStationsCount = stations.Count,
                ActiveStationsCount = stations.Count(s => s.IsActive),
                FaultyStationsCount = stations.Count(s => s.CurrentStatus == StationStatuses.Faulty),
                TotalBookings = totalBookings,
                AverageRating = avgRating,
                Stations = stations,
                RecentReviews = recentReviews,
                RecentFaultReports = recentFaults
            };
        }
    }
}
