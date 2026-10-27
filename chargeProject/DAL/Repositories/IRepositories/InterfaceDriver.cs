using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceDriver
    {
        Task<Drivers?> GetByIdAsync(int id);
        Task<List<Drivers>> GetAllAsync();
        Task<Drivers?> GetByUserIdAsync(int userId);
        Task<Drivers> AddAsync(Drivers entity);
        Task<bool> DeleteAsync(int id);

        // פונקציות עזר לוולידציה (שעברו מה-Repository הישן)
        Task<bool> UserExistsAsync(int userId);
        Task<bool> DriverExistsForUserAsync(int userId);
    }
}