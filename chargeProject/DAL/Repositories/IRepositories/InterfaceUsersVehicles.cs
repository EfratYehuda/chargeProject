using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceUsersVehicles
    {
        Task<UsersVehicles?> GetByIdAsync(int id);
        Task<List<UsersVehicles>> GetAllAsync();
        Task<List<UsersVehicles>> GetByDriverIdAsync(int driverId);
        Task<UsersVehicles> AddAsync(UsersVehicles entity);
        Task<UsersVehicles> UpdateAsync(UsersVehicles entity);
        Task<bool> DeleteAsync(int id);

        // פונקציות עזר לאבטחת IDOR:
        Task<UsersVehicles?> GetOwnedAsync(int id, int driverId);
        Task<int?> GetDriverIdByUserIdAsync(int userId);

        // פונקציית עזר לוולידציה
        Task<bool> DriverExistsAsync(int driverId);
    }
}