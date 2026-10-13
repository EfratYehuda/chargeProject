using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Repositories.IRepositories;
using DAL;
using DAL.Classes;
using DAL.Data;
using DTO;
using DTO.USERDTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה לנתוני נהגים</summary>
    public class DriversRepository : InterfaceDriver
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DriversRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Drivers> Drivers => _context.Set<Drivers>().AsNoTracking();

        public async Task<DriverDto?> GetByIdAsync(int id) =>
            await Drivers.Where(d => d.Id == id)
                .ProjectTo<DriverDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<DriverDto>> GetAllAsync() =>
            await Drivers.ProjectTo<DriverDto>(_mapper.ConfigurationProvider).ToListAsync();

        public async Task<DriverDto?> GetByUserIdAsync(int userId) =>
            await Drivers.Where(d => d.UserId == userId)
                .ProjectTo<DriverDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<DriverDto> AddAsync(CreateDriverDto dto)
        {
            if (!await _context.Set<Users>().AnyAsync(u => u.Id == dto.UserId))
                throw new KeyNotFoundException($"משתמש {dto.UserId} לא נמצא");
            if (await _context.Set<Drivers>().AnyAsync(d => d.UserId == dto.UserId))
                throw new InvalidOperationException("למשתמש כבר קיים פרופיל נהג");

            var entity = _mapper.Map<Drivers>(dto);
            _context.Set<Drivers>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Drivers>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<Drivers>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
