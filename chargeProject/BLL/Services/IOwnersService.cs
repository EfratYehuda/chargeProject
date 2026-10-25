using DTO.USERDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IOwnersService
    {
        Task<OwnerDto?> GetByIdAsync(int id);
        Task<List<OwnerDto>> GetAllAsync();
        Task<OwnerDto?> GetByUserIdAsync(int userId);
        Task<OwnerDto> AddAsync(CreateOwnerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<OwnerDashboardDto?> GetDashboardAsync(int ownerId);
    }
}