using DTO;
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Repositories.IRepositories
{
    // <summary>גישה לנתוני נהגים. אין Update כי לטבלה אין שדות ניתנים לעדכון (פרטים אישיים מתעדכנים דרך Users).</summary>
    public interface InterfaceDriver
    {
        Task<DriverDto?> GetByIdAsync(int id);
        Task<List<DriverDto>> GetAllAsync();
        Task<DriverDto?> GetByUserIdAsync(int userId);
        Task<DriverDto> AddAsync(CreateDriverDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
