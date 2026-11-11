using AutoMapper;
using BLL.Exceptions;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO;
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

        public async Task<ReviewDto> GetByIdAsync(int id)
        {
            var entity = await _reviewRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("הביקורת לא נמצאה");
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
            return entity == null ? null : _mapper.Map<ReviewDto>(entity);
        }

        public async Task<double?> GetStationAverageRatingAsync(int stationId)
        {
            return await _reviewRepo.GetStationAverageRatingAsync(stationId);
        }

        public async Task<ReviewDto> AddAsync(int driverId, CreateReviewDto dto)
        {
            // A booking that is not yours looks exactly like a booking that does not exist.
            var booking = await _reviewRepo.GetBookingByIdAsync(dto.ChargeBookingId);
            if (booking == null || booking.DriverId != driverId)
                throw new NotFoundException("ההזמנה לא נמצאה");

            if (booking.Status != BookingStatuses.Completed)
                throw new BusinessRuleException("ניתן לדרג רק טעינה שהסתיימה");

            if (await _reviewRepo.ReviewExistsForBookingAsync(dto.ChargeBookingId))
                throw new ConflictException("כבר נכתבה ביקורת להזמנה זו");

            var entity = _mapper.Map<ReviewsToCharge>(dto);
            entity.DriverId = driverId;   // from the token, not from the client
            entity.CreatedAt = DateTime.Now;

            var savedEntity = await _reviewRepo.AddAsync(entity);
            var fullEntity = await _reviewRepo.GetByIdAsync(savedEntity.Id);

            return _mapper.Map<ReviewDto>(fullEntity!);
        }

        // IDOR protection: only the driver who wrote the review can delete it.
        public async Task DeleteAsync(int id, int driverId)
        {
            var entity = await _reviewRepo.GetOwnedAsync(id, driverId)
                ?? throw new NotFoundException("הביקורת לא נמצאה");

            await _reviewRepo.DeleteAsync(entity.Id);
        }
    }
}