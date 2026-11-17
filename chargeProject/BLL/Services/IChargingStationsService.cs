using DTO.STATIONDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChargingStationsService
    {
        // Reads: stations are public data for any logged-in user (approved only).
        Task<StationDto> GetByIdAsync(int id);
        Task<List<StationDto>> GetAllAsync();
        Task<List<StationDto>> GetAllForAdminAsync();
        Task<List<StationDto>> GetPendingForAdminAsync();
        Task<List<StationDto>> GetByOwnerIdAsync(int ownerId);
        Task<List<StationDto>> GetActiveInRadiusAsync(double latitude, double longitude, double radiusKm, string? chargerType);

        // Writes. ownerId always comes from the token, never from the request body.
        Task<StationDto> AddAsync(int ownerId, CreateUpdateStationDto dto);
        Task<StationDto> UpdateAsync(int id, int ownerId, CreateUpdateStationDto dto);
        Task<StationDto> SetActiveAsync(int id, int ownerId, bool isActive);
        Task<StationDto> UpdateBlockedHoursAsync(int id, int ownerId, List<int> blockedHours);
        Task<StationDto> ReviewApprovalAsync(int id, ReviewStationApprovalDto dto);
        Task DeleteAsync(int id, int ownerId);
        Task<List<StationDto>> SearchAsync(string? text, int max = 5);

        // המלצות (האלגוריתם): מצב רדיוס – סביב הנהג, ומצב מסלול – עצירה בדרך ליעד
        Task<List<RankedStationResultDto>> RecommendAsync(DriverPreferenceRequestDto request);
        Task<RouteRecommendationResponseDto> RecommendRouteAsync(DriverPreferenceRequestDto request);
    }
}