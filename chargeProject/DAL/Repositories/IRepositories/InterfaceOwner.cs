using DAL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepositories
{
    public interface InterfaceOwner
    {
        Task<Owner?> GetByIdAsync(int id);
        Task<List<Owner>> GetAllAsync();
        Task<Owner?> GetByUserIdAsync(int userId);
        Task<Owner> AddAsync(Owner entity);
        Task<bool> DeleteAsync(int id);

        // עזרים עבור הדאשבורד ולוגיקה כללית
        Task<bool> ExistsAsync(int ownerId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> OwnerExistsForUserAsync(int userId);
        Task<List<ChargingStations>> GetStationsByOwnerIdAsync(int ownerId);
        Task<List<ReviewsToCharge>> GetRecentReviewsByOwnerIdAsync(int ownerId, int count);
        Task<List<StationStatusLogs>> GetRecentFaultsByOwnerIdAsync(int ownerId, int count, string faultyStatus);
        Task<int> GetTotalBookingsByOwnerIdAsync(int ownerId);
        Task<double?> GetAverageRatingByOwnerIdAsync(int ownerId);
    }
}