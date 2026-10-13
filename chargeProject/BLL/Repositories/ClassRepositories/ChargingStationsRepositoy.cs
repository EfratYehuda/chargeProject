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
    /// <summary>מימוש גישה לעמדות טעינה</summary>
    public class ChargingStationsRepository : InterfaceChargingStations
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ChargingStationsRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<ChargingStations> Stations => _context.Set<ChargingStations>().AsNoTracking();

        public async Task<StationDto?> GetByIdAsync(int id) =>
            await Stations.Where(s => s.Id == id)
                .ProjectTo<StationDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<StationDto>> GetAllAsync() =>
            await Stations.ProjectTo<StationDto>(_mapper.ConfigurationProvider).ToListAsync();

        public async Task<List<StationDto>> GetByOwnerIdAsync(int ownerId) =>
            await Stations.Where(s => s.OwnerId == ownerId)
                .ProjectTo<StationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType)
        {
            // ריבוע תוחם (כ-111 ק"מ לכל מעלת רוחב). חישוב מרחק מדויק (Haversine) ייעשה בשכבת השירות.
            var latDelta = radiusKm / 111.0;
            var lngDelta = radiusKm / (111.0 * Math.Max(Math.Cos(latitude * Math.PI / 180.0), 0.01));

            var query = Stations.Where(s => s.IsActive
                && s.Latitude >= latitude - latDelta && s.Latitude <= latitude + latDelta
                && s.Longitude >= longitude - lngDelta && s.Longitude <= longitude + lngDelta);

            if (!string.IsNullOrWhiteSpace(chargerType))
                query = query.Where(s => s.ChargerType == chargerType);

            return await query.ProjectTo<StationDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<StationDto> AddAsync(CreateUpdateStationDto dto)
        {
            if (!await _context.Set<Owner>().AnyAsync(o => o.Id == dto.OwnerId))
                throw new KeyNotFoundException($"בעל עמדות {dto.OwnerId} לא נמצא");

            var entity = _mapper.Map<ChargingStations>(dto);
            _context.Set<ChargingStations>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<StationDto?> UpdateAsync(int id, CreateUpdateStationDto dto)
        {
            var entity = await _context.Set<ChargingStations>().FindAsync(id);
            if (entity == null) return null;

            var originalOwnerId = entity.OwnerId;     // בעלות על עמדה לא משתנה בעדכון רגיל
            _mapper.Map(dto, entity);
            entity.OwnerId = originalOwnerId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<StationDto?> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _context.Set<ChargingStations>().FindAsync(id);
            if (entity == null) return null;

            entity.IsActive = isActive;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ChargingStations>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ChargingStations>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
