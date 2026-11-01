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

        // החתימות המעודכנות שכוללות ownerId לאבטחת IDOR:
        Task<StationDto?> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto);
        Task<StationDto?> SetActiveAsync(int id, int ownerId, bool isActive);
        Task<bool> DeleteAsync(int id, int ownerId);

        // מתודת עזר לחיפוש ה-OwnerId לפי ה-UserId מהטוקן:
        Task<int?> GetOwnerIdByUserIdAsync(int userId);
    }
}