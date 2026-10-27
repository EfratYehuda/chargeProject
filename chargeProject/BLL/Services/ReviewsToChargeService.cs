using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // עבור BookingStatuses
using DTO.REVIEWDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ReviewsToChargeService : IReviewsToChargeService
    {
        private readonly InterfaceReviewsToCharge _reviewRepo;
        private readonly IMapper _mapper;

        public ReviewsToChargeService(InterfaceReviewsToCharge reviewRepo, IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _mapper = mapper;
        }

        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            var entity = await _reviewRepo.GetByIdAsync(id);
            return _mapper.Map<ReviewDto>(entity);
        }

        public async Task<List<ReviewDto>> GetAllAsync()
        {
            var entities = await _reviewRepo.GetAllAsync();
            return _mapper.Map<List<ReviewDto>>(entities);
        }

        public async Task<List<ReviewDto>> GetByStationIdAsync(int stationId)
        {
            var entities = await _reviewRepo.GetByStationIdAsync(stationId);
            return _mapper.Map<List<ReviewDto>>(entities);
        }

        public async Task<List<ReviewDto>> GetByDriverIdAsync(int driverId)
        {
            var entities = await _reviewRepo.GetByDriverIdAsync(driverId);
            return _mapper.Map<List<ReviewDto>>(entities);
        }

        public async Task<List<ReviewDto>> GetByOwnerIdAsync(int ownerId)
        {
            var entities = await _reviewRepo.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<List<ReviewDto>>(entities);
        }

        public async Task<ReviewDto?> GetByBookingIdAsync(int bookingId)
        {
            var entity = await _reviewRepo.GetByBookingIdAsync(bookingId);
            return _mapper.Map<ReviewDto>(entity);
        }

        public async Task<double?> GetStationAverageRatingAsync(int stationId)
        {
            return await _reviewRepo.GetStationAverageRatingAsync(stationId);
        }

        public async Task<ReviewDto> AddAsync(CreateReviewDto dto)
        {
            var booking = await _reviewRepo.GetBookingByIdAsync(dto.ChargeBookingId)
                ?? throw new KeyNotFoundException($"הזמנה {dto.ChargeBookingId} לא נמצאה");

            if (booking.DriverId != dto.DriverId)
                throw new InvalidOperationException("ההזמנה אינה שייכת לנהג");

            if (booking.Status != BookingStatuses.Completed)
                throw new InvalidOperationException("ניתן לדרג רק טעינה שהסתיימה");

            if (await _reviewRepo.ReviewExistsForBookingAsync(dto.ChargeBookingId))
                throw new InvalidOperationException("כבר נכתבה ביקורת להזמנה זו");

            var entity = _mapper.Map<ReviewsToCharge>(dto);
            entity.CreatedAt = DateTime.Now;

            var savedEntity = await _reviewRepo.AddAsync(entity);
            var fullEntity = await _reviewRepo.GetByIdAsync(savedEntity.Id);

            return _mapper.Map<ReviewDto>(fullEntity!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _reviewRepo.DeleteAsync(id);
        }
    }
}