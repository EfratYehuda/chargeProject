using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceReviewsToCharge
    {
        Task<ReviewsToCharge?> GetByIdAsync(int id);
        Task<List<ReviewsToCharge>> GetAllAsync();
        Task<List<ReviewsToCharge>> GetByStationIdAsync(int stationId);
        Task<List<ReviewsToCharge>> GetByDriverIdAsync(int driverId);
        Task<List<ReviewsToCharge>> GetByOwnerIdAsync(int ownerId);
        Task<ReviewsToCharge?> GetByBookingIdAsync(int bookingId);
        Task<double?> GetStationAverageRatingAsync(int stationId);
        Task<ReviewsToCharge> AddAsync(ReviewsToCharge entity);
        Task<bool> DeleteAsync(int id);

        // --- פונקציות עזר לאבטחת IDOR ---
        Task<ReviewsToCharge?> GetOwnedAsync(int id, int driverId);
        Task<int?> GetDriverIdByUserIdAsync(int userId);

        // עזר לוולידציה
        Task<ChargeBooking?> GetBookingByIdAsync(int bookingId);
        Task<bool> ReviewExistsForBookingAsync(int bookingId);
    }
}