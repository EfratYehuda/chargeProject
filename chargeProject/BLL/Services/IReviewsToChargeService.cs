using DTO.REVIEWDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IReviewsToChargeService
    {
        Task<ReviewDto> GetByIdAsync(int id);
        Task<List<ReviewDto>> GetAllAsync();                    // admin only (enforced in the controller)
        Task<List<ReviewDto>> GetByStationIdAsync(int stationId);
        Task<List<ReviewDto>> GetByDriverIdAsync(int driverId);
        Task<List<ReviewDto>> GetByOwnerIdAsync(int ownerId);
        Task<ReviewDto?> GetByBookingIdAsync(int bookingId);    // null = "no review yet" is a normal answer here
        Task<double?> GetStationAverageRatingAsync(int stationId);

        // driverId always comes from the token, never from the request body.
        Task<ReviewDto> AddAsync(int driverId, CreateReviewDto dto);
        Task DeleteAsync(int id, int driverId);
    }
}