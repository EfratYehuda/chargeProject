using DTO.USERDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IDriversService
    {
        Task<DriverDto> GetByIdAsync(int id);
        Task<List<DriverDto>> GetAllAsync();
        Task<DriverDto> GetByUserIdAsync(int userId);
        Task<DriverDto> AddAsync(CreateDriverDto dto);   // admin only (enforced in the controller)
        Task DeleteAsync(int id);
    }
}