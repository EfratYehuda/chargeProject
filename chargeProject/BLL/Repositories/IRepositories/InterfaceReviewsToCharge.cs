using DTO;
using DTO.REVIEWDTO;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLL.Repositories.IRepositories
{
    /// <summary>גישה לביקורות על טעינות</summary>
    public interface InterfaceReviewsToCharge
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
