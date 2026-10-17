using DTO.USERDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetByIdAsync(int id);
        Task<List<UserProfileDto>> GetAllAsync();
        Task<UserProfileDto?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<UserProfileDto> AddAsync(RegisterRequestDto dto, string passwordHash);
        Task<UserProfileDto?> UpdateAsync(int id, UpdateUserProfileDto dto);
        Task<bool> DeleteAsync(int id);
    }
}