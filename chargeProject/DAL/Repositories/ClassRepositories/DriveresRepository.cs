using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class DriversRepository : InterfaceDriver
    {
        private readonly AppDbContext _context;

        public DriversRepository(AppDbContext context)
        {
            _context = context;
        }

        // כולל את הטבלאות הקשורות כדי שהמאפר יוכל לגשת לשם המשתמש ולספור הזמנות ורכבים
        private IQueryable<Drivers> DriversQuery => _context.Set<Drivers>()
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .Include(d => d.Bookings)
            .AsNoTracking();

        public async Task<Drivers?> GetByIdAsync(int id) =>
            await DriversQuery.FirstOrDefaultAsync(d => d.Id == id);

        public async Task<List<Drivers>> GetAllAsync() =>
            await DriversQuery.ToListAsync();

        public async Task<Drivers?> GetByUserIdAsync(int userId) =>
            await DriversQuery.FirstOrDefaultAsync(d => d.UserId == userId);

        public async Task<Drivers> AddAsync(Drivers entity)
        {
            _context.Set<Drivers>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Drivers>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<Drivers>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Set<Users>().AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> DriverExistsForUserAsync(int userId)
        {
            return await _context.Set<Drivers>().AnyAsync(d => d.UserId == userId);
        }
    }
}