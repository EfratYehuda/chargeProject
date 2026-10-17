using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class UserRepository : InterfaceUser
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Users> UsersQuery => _context.Set<Users>()
            .Include(u => u.DriverProfile)
            .Include(u => u.OwnerProfile)
            .AsNoTracking();

        public async Task<Users?> GetByIdAsync(int id) =>
            await UsersQuery.FirstOrDefaultAsync(u => u.Id == id);

        public async Task<List<Users>> GetAllAsync() =>
            await UsersQuery.ToListAsync();

        public async Task<Users?> GetByEmailAsync(string email) =>
            await UsersQuery.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> EmailExistsAsync(string email) =>
            await _context.Set<Users>().AnyAsync(u => u.Email == email);

        public async Task<Users> AddAsync(Users entity)
        {
            _context.Set<Users>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Users> UpdateAsync(Users entity)
        {
            _context.Set<Users>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
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