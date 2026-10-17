using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // עבור StationStatuses
using DTO.REVIEWDTO;
using DTO.STATIONDTO;
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OwnersService : IOwnersService
    {
        private readonly InterfaceOwner _ownerRepo;
        private readonly IMapper _mapper;

        public OwnersService(InterfaceOwner ownerRepo, IMapper mapper)
        {
            _ownerRepo = ownerRepo;
            _mapper = mapper;
        }

        public async Task<OwnerDto?> GetByIdAsync(int id)
        {
            var entity = await _ownerRepo.GetByIdAsync(id);
            return _mapper.Map<OwnerDto>(entity);
        }

        public async Task<List<OwnerDto>> GetAllAsync()
        {
            var entities = await _ownerRepo.GetAllAsync();
            return _mapper.Map<List<OwnerDto>>(entities);
        }

        public async Task<OwnerDto?> GetByUserIdAsync(int userId)
        {
            var entity = await _ownerRepo.GetByUserIdAsync(userId);
            return _mapper.Map<OwnerDto>(entity);
        }

        public async Task<OwnerDto> AddAsync(CreateOwnerDto dto)
        {
            if (!await _ownerRepo.UserExistsAsync(dto.UserId))
                throw new KeyNotFoundException($"משתמש {dto.UserId} לא נמצא");

            if (await _ownerRepo.OwnerExistsForUserAsync(dto.UserId))
                throw new InvalidOperationException("למשתמש כבר קיים פרופיל בעל עמדות");

            var entity = _mapper.Map<Owner>(dto);
            var savedEntity = await _ownerRepo.AddAsync(entity);

            var fullEntity = await _ownerRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<OwnerDto>(fullEntity!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _ownerRepo.DeleteAsync(id);
        }

        public async Task<OwnerDashboardDto?> GetDashboardAsync(int ownerId)
        {
            if (!await _ownerRepo.ExistsAsync(ownerId))
                return null;

            // משיכת כל הנתונים מה-DAL
            var stationsEntities = await _ownerRepo.GetStationsByOwnerIdAsync(ownerId);
            var recentReviewsEntities = await _ownerRepo.GetRecentReviewsByOwnerIdAsync(ownerId, 10);

            // הנחה ש-StationStatuses.Faulty מוגדר ב-DTO, אחרת צריך להחליף במחרוזת המתאימה
            var recentFaultsEntities = await _ownerRepo.GetRecentFaultsByOwnerIdAsync(ownerId, 10, StationStatuses.Faulty);

            var totalBookings = await _ownerRepo.GetTotalBookingsByOwnerIdAsync(ownerId);
            var avgRating = await _ownerRepo.GetAverageRatingByOwnerIdAsync(ownerId);

            // מיפוי הנתונים ל-DTOs
            var stations = _mapper.Map<List<StationDto>>(stationsEntities);
            var recentReviews = _mapper.Map<List<ReviewDto>>(recentReviewsEntities);
            var recentFaults = _mapper.Map<List<StationStatusLogDto>>(recentFaultsEntities);

            return new OwnerDashboardDto
            {
                OwnerId = ownerId,
                TotalStationsCount = stations.Count,
                ActiveStationsCount = stations.Count(s => s.IsActive),
                FaultyStationsCount = stations.Count(s => s.CurrentStatus == StationStatuses.Faulty),
                TotalBookings = totalBookings,
                AverageRating = avgRating,
                Stations = stations,
                RecentReviews = recentReviews,
                RecentFaultReports = recentFaults
            };
        }
    }
}