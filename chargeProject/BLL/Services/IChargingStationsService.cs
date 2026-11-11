using DTO.STATIONDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChargingStationsService
    {
        // Reads: stations are public data for any logged-in user (drivers search for them).
        Task<StationDto> GetByIdAsync(int id);
        Task<List<StationDto>> GetAllAsync();
        Task<List<StationDto>> GetByOwnerIdAsync(int ownerId);
        Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType);

        // Writes. ownerId always comes from the token, never from the request body.
        Task<StationDto> AddAsync(int ownerId, CreateUpdateStationDto dto);
        Task<StationDto> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto);
        Task<StationDto> SetActiveAsync(int id, int ownerId, bool isActive);
        Task DeleteAsync(int id, int ownerId);
    }
}