using DTO;
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה לנתוני בעלי עמדות + לוח הבקרה שלהם</summary>
    public interface InterfaceOwner
    {
        Task<OwnerDto?> GetByIdAsync(int id);
        Task<List<OwnerDto>> GetAllAsync();
        Task<OwnerDto?> GetByUserIdAsync(int userId);
        Task<OwnerDto> AddAsync(CreateOwnerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<OwnerDashboardDto?> GetDashboardAsync(int ownerId);
    }

}
