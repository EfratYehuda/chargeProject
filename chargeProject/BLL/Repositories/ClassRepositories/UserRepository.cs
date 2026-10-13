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
    /// <summary>מימוש גישה לנתוני משתמשים</summary>
    public class UserRepository : InterfaceUser
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Users> Users => _context.Set<Users>().AsNoTracking();

        public async Task<UserProfileDto?> GetByIdAsync(int id) =>
            await Users.Where(u => u.Id == id)
                .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<UserProfileDto>> GetAllAsync() =>
            await Users.ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider).ToListAsync();

        public async Task<UserProfileDto?> GetByEmailAsync(string email) =>
            await Users.Where(u => u.Email == email)
                .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<bool> EmailExistsAsync(string email) =>
            await Users.AnyAsync(u => u.Email == email);

        public async Task<UserProfileDto> AddAsync(RegisterRequestDto dto, string passwordHash)
        {
            var entity = _mapper.Map<Users>(dto);
            entity.PasswordHash = passwordHash;

            // יוצרים את הפרופיל המתאים בהתאם לתפקיד שנבחר במסך הכניסה
            if (dto.Role == UserRoles.Driver) entity.DriverProfile = new Drivers();
            else if (dto.Role == UserRoles.Owner) entity.OwnerProfile = new Owner();

            _context.Set<Users>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<UserProfileDto?> UpdateAsync(int id, UpdateUserProfileDto dto)
        {
            var entity = await _context.Set<Users>().FindAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Users>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<Users>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
