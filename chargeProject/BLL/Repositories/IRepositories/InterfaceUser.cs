using DTO;
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה לנתוני משתמשים (הרשמה, התחברות, פרופיל)</summary>
    public interface InterfaceUser
    {
        Task<UserProfileDto?> GetByIdAsync(int id);
        Task<List<UserProfileDto>> GetAllAsync();
        Task<UserProfileDto?> GetByEmailAsync(string email);
        /// <summary>מחזיר hash לצורך אימות סיסמה - לשימוש פנימי בלבד</summary>
        //Task<UserCredentialsDto?> GetCredentialsByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        /// <summary>יוצר משתמש + פרופיל נהג/בעלים לפי Role. ה-hash מגיע מבחוץ.</summary>
        Task<UserProfileDto> AddAsync(RegisterRequestDto dto, string passwordHash);
        Task<UserProfileDto?> UpdateAsync(int id, UpdateUserProfileDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
