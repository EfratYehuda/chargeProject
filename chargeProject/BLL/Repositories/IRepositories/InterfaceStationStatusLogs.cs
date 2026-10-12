using DTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה ללוג סטטוסים ודיווחי תקלות. הלוג הוא Append-only, לכן אין Update.</summary>
    public interface InterfaceStationStatusLogs
    {
        Task<StationStatusLogDto?> GetByIdAsync(int id);
        Task<List<StationStatusLogDto>> GetAllAsync();
        Task<List<StationStatusLogDto>> GetByStationIdAsync(int stationId);
        Task<StationStatusLogDto?> GetLatestByStationIdAsync(int stationId);
        Task<List<StationStatusLogDto>> GetFaultReportsByOwnerIdAsync(int ownerId, int take = 20);
        //Task<StationStatusLogDto> AddAsync(CreateStationStatusLogDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
