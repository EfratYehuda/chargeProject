using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.IRepositories;
using DAL;
using DAL.Classes;
using DAL.Data;
using DTO;
using DTO.STATIONDTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה ללוג סטטוסים ודיווחי תקלות</summary>
    public class StationStatusLogsRepository : InterfaceStationStatusLogs
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public StationStatusLogsRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<StationStatusLogs> Logs => _context.Set<StationStatusLogs>().AsNoTracking();

        public async Task<StationStatusLogDto?> GetByIdAsync(int id) =>
            await Logs.Where(l => l.Id == id)
                .ProjectTo<StationStatusLogDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<StationStatusLogDto>> GetAllAsync() =>
            await Logs.OrderByDescending(l => l.UpdatedAt)
                .ProjectTo<StationStatusLogDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<StationStatusLogDto>> GetByStationIdAsync(int stationId) =>
            await Logs.Where(l => l.StationId == stationId)
                .OrderByDescending(l => l.UpdatedAt)
                .ProjectTo<StationStatusLogDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<StationStatusLogDto?> GetLatestByStationIdAsync(int stationId) =>
            await Logs.Where(l => l.StationId == stationId)
                .OrderByDescending(l => l.UpdatedAt)
                .ProjectTo<StationStatusLogDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<StationStatusLogDto>> GetFaultReportsByOwnerIdAsync(int ownerId, int take = 20) =>
            await Logs.Where(l => l.Station.OwnerId == ownerId && l.Status == StationStatuses.Faulty)
                .OrderByDescending(l => l.UpdatedAt)
                .Take(take)
                .ProjectTo<StationStatusLogDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<StationStatusLogDto> AddAsync(CreateStationStatusLogDto dto)
        {
            if (!await _context.Set<ChargingStations>().AnyAsync(s => s.Id == dto.StationId))
                throw new KeyNotFoundException($"עמדה {dto.StationId} לא נמצאה");

            if (dto.CurrentDriverId.HasValue &&
                !await _context.Set<Drivers>().AnyAsync(d => d.Id == dto.CurrentDriverId.Value))
                throw new KeyNotFoundException($"נהג {dto.CurrentDriverId} לא נמצא");

            var entity = _mapper.Map<StationStatusLogs>(dto);
            entity.UpdatedAt = DateTime.Now;

            _context.Set<StationStatusLogs>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<StationStatusLogs>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<StationStatusLogs>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
