using DTO.STATIONDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChargingStationsService
    {
        Task<StationDto?> GetByIdAsync(int id);
        Task<List<StationDto>> GetAllAsync();
        Task<List<StationDto>> GetByOwnerIdAsync(int ownerId);
        Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType);
        Task<StationDto> AddAsync(CreateUpdateStationDto dto);
        Task<StationDto?> UpdateAsync(int id, CreateUpdateStationDto dto);
        Task<StationDto?> SetActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}