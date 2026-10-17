using DTO.STATIONDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IStationStatusLogsService
    {
        Task<StationStatusLogDto?> GetByIdAsync(int id);
        Task<List<StationStatusLogDto>> GetAllAsync();
        Task<List<StationStatusLogDto>> GetByStationIdAsync(int stationId);
        Task<StationStatusLogDto?> GetLatestByStationIdAsync(int stationId);
        Task<List<StationStatusLogDto>> GetFaultReportsByOwnerIdAsync(int ownerId, int take = 20);
        Task<StationStatusLogDto> AddAsync(CreateStationStatusLogDto dto);
        Task<bool> DeleteAsync(int id);
    }
}