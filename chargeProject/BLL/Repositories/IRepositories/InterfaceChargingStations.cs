using DTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה לעמדות טעינה (ניהול + חיפוש)</summary>
    public interface InterfaceChargingStations
    {
        Task<StationDto?> GetByIdAsync(int id);
        Task<List<StationDto>> GetAllAsync();
        Task<List<StationDto>> GetByOwnerIdAsync(int ownerId);
        /// <summary>עמדות פעילות בריבוע סביב נקודה (סינון ראשוני ב-DB; חישוב מרחק מדויק ומיון ב-Service)</summary>
        Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType);
        Task<StationDto> AddAsync(CreateUpdateStationDto dto);
        Task<StationDto?> UpdateAsync(int id, CreateUpdateStationDto dto);
        Task<StationDto?> SetActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}
