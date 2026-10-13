using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.IRepositories;
using DAL;
using DAL.Classes;
using DAL.Data;
using DTO;
using DTO.VEHICLESDTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה לרכבים של נהגים</summary>
    public class UsersVehiclesRepository : InterfaceUsersVehicles
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UsersVehiclesRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<UsersVehicles> Vehicles => _context.Set<UsersVehicles>().AsNoTracking();

        public async Task<VehicleDto?> GetByIdAsync(int id) =>
            await Vehicles.Where(v => v.Id == id)
                .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<VehicleDto>> GetAllAsync() =>
            await Vehicles.ProjectTo<VehicleDto>(_mapper.ConfigurationProvider).ToListAsync();

        public async Task<List<VehicleDto>> GetByDriverIdAsync(int driverId) =>
            await Vehicles.Where(v => v.DriverId == driverId)
                .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<VehicleDto> AddAsync(CreateUpdateVehicleDto dto)
        {
            if (!await _context.Set<Drivers>().AnyAsync(d => d.Id == dto.DriverId))
                throw new KeyNotFoundException($"נהג {dto.DriverId} לא נמצא");

            var entity = _mapper.Map<UsersVehicles>(dto);
            _context.Set<UsersVehicles>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<VehicleDto?> UpdateAsync(int id, CreateUpdateVehicleDto dto)
        {
            var entity = await _context.Set<UsersVehicles>().FindAsync(id);
            if (entity == null) return null;

            var originalDriverId = entity.DriverId;   // לא מאפשרים "העברת" רכב לנהג אחר
            _mapper.Map(dto, entity);
            entity.DriverId = originalDriverId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<VehicleDto?> UpdateCurrentBatteryAsync(int id, double batteryPercent)
        {
            if (batteryPercent < 0 || batteryPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(batteryPercent), "אחוז סוללה חייב להיות בין 0 ל-100");

            var entity = await _context.Set<UsersVehicles>().FindAsync(id);
            if (entity == null) return null;

            entity.CurrentBattery = batteryPercent;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<UsersVehicles>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<UsersVehicles>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
