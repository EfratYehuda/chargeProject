using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class UsersVehiclesRepository : InterfaceUsersVehicles
    {
        private readonly AppDbContext _context;

        public UsersVehiclesRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<UsersVehicles> VehiclesQuery => _context.Set<UsersVehicles>()
            .Include(v => v.Driver).ThenInclude(d => d.User)
            .AsNoTracking();

        public async Task<UsersVehicles?> GetByIdAsync(int id) =>
            await VehiclesQuery.FirstOrDefaultAsync(v => v.Id == id);

        public async Task<List<UsersVehicles>> GetAllAsync() =>
            await VehiclesQuery.ToListAsync();

        public async Task<List<UsersVehicles>> GetByDriverIdAsync(int driverId) =>
            await VehiclesQuery.Where(v => v.DriverId == driverId).ToListAsync();

        public async Task<UsersVehicles> AddAsync(UsersVehicles entity)
        {
            _context.Set<UsersVehicles>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<UsersVehicles> UpdateAsync(UsersVehicles entity)
        {
            _context.Set<UsersVehicles>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<UsersVehicles>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<UsersVehicles>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DriverExistsAsync(int driverId) =>
            await _context.Set<Drivers>().AnyAsync(d => d.Id == driverId);
    }
}