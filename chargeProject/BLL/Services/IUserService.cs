using DTO.USERDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUserService
    {
        // Create a driver / owner profile for the logged-in user and return a NEW token that contains the new profile id.
        Task<AuthResponseDto> BecomeOwnerAsync(int userId);
        Task<AuthResponseDto> BecomeDriverAsync(int userId);

        // null = wrong email or password. This is an expected result, not an exception.
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);

        Task<UserProfileDto> GetByIdAsync(int id);
        Task<List<UserProfileDto>> GetAllAsync();
        Task<UserProfileDto?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<UserProfileDto> AddAsync(RegisterRequestDto dto);
        Task<UserProfileDto> UpdateAsync(int id, UpdateUserProfileDto dto);
        Task DeleteAsync(int id);
    }
}