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

        // עזר לוולידציה (הועבר מהקוד הישן)
        Task<ChargeBooking?> GetBookingByIdAsync(int bookingId);
        Task<bool> ReviewExistsForBookingAsync(int bookingId);
    }
}