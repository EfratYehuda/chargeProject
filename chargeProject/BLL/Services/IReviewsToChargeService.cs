using DTO.REVIEWDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IReviewsToChargeService
    {
        Task<ReviewDto?> GetByIdAsync(int id);
        Task<List<ReviewDto>> GetAllAsync();
        Task<List<ReviewDto>> GetByStationIdAsync(int stationId);
        Task<List<ReviewDto>> GetByDriverIdAsync(int driverId);
        Task<List<ReviewDto>> GetByOwnerIdAsync(int ownerId);
        Task<ReviewDto?> GetByBookingIdAsync(int bookingId);
        Task<double?> GetStationAverageRatingAsync(int stationId);
        Task<ReviewDto> AddAsync(CreateReviewDto dto);
        Task<bool> DeleteAsync(int id);
    }
}