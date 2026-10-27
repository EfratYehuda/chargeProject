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

        // פונקציית עזר לוולידציה
        Task<bool> DriverExistsAsync(int driverId);
    }
}