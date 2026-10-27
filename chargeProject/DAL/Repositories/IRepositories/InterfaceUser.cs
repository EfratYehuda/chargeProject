using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceUser
    {
        Task<Users?> GetByIdAsync(int id);
        Task<List<Users>> GetAllAsync();
        Task<Users?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<Users> AddAsync(Users entity);
        Task<Users> UpdateAsync(Users entity);
        Task<bool> DeleteAsync(int id);
    }
}