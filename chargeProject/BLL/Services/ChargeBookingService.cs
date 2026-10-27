using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ChargeBookingService : IChargeBookingService
    {
        private static readonly string[] ActiveStatuses =
            { BookingStatuses.Held, BookingStatuses.Confirmed, BookingStatuses.Charging };

        private readonly InterfaceChargeBooking _bookingRepo;
        private readonly IMapper _mapper;

        public ChargeBookingService(InterfaceChargeBooking bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<ChargeBookingDto?> GetByIdAsync(int id)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            return _mapper.Map<ChargeBookingDto>(entity);
        }

        public async Task<List<ChargeBookingDto>> GetAllAsync()
        {
            var entities = await _bookingRepo.GetAllAsync();
            return _mapper.Map<List<ChargeBookingDto>>(entities);
        }

        public async Task<List<ChargeBookingDto>> GetByDriverIdAsync(int driverId)
        {
            var entities = await _bookingRepo.GetByDriverIdAsync(driverId);
            return _mapper.Map<List<ChargeBookingDto>>(entities);
        }

        public async Task<List<ChargeBookingDto>> GetByStationIdAsync(int stationId)
        {
            var entities = await _bookingRepo.GetByStationIdAsync(stationId);
            return _mapper.Map<List<ChargeBookingDto>>(entities);
        }

        public async Task<List<ChargeBookingDto>> GetByOwnerIdAsync(int ownerId)
        {
            var entities = await _bookingRepo.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<List<ChargeBookingDto>>(entities);
        }

        public async Task<ChargeBookingDto> AddAsync(CreateChargeBookingDto dto)
        {
            // בדיקות הלוגיקה שהיו קודם ב-Repository עברו לכאן
            var station = await _bookingRepo.GetStationByIdAsync(dto.StationId)
                ?? throw new KeyNotFoundException($"עמדה {dto.StationId} לא נמצאה");
            if (!station.IsActive)
                throw new InvalidOperationException("העמדה אינה פעילה");

            var vehicle = await _bookingRepo.GetVehicleByIdAsync(dto.VehicleId)
                ?? throw new KeyNotFoundException($"רכב {dto.VehicleId} לא נמצא");
            if (vehicle.DriverId != dto.DriverId)
                throw new InvalidOperationException("הרכב אינו שייך לנהג");

            if (await _bookingRepo.HasActiveBookingAsync(dto.VehicleId, ActiveStatuses))
                throw new InvalidOperationException("לרכב כבר קיימת הזמנה פעילה");

            var entity = _mapper.Map<ChargeBooking>(dto);
            entity.Status = BookingStatuses.Held;
            entity.CreatedAt = DateTime.Now;

            var savedEntity = await _bookingRepo.AddAsync(entity);
            return _mapper.Map<ChargeBookingDto>(savedEntity);
        }

        public async Task<ChargeBookingDto?> UpdateAsync(int id, CreateChargeBookingDto dto)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.CurrentBatteryPercent = dto.CurrentBatteryPercent;
            entity.TargetBatteryPercent = dto.TargetBatteryPercent;
            entity.EstimatedArrivalTime = dto.EstimatedArrivalTime;

            var updatedEntity = await _bookingRepo.UpdateAsync(entity);
            return _mapper.Map<ChargeBookingDto>(updatedEntity);
        }

        public async Task<ChargeBookingDto?> UpdateStatusAsync(int id, string status)
        {
            var entity = await _bookingRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Status = status;
            var updatedEntity = await _bookingRepo.UpdateAsync(entity);
            return _mapper.Map<ChargeBookingDto>(updatedEntity);
        }

        public async Task<int> ExpireStaleHoldsAsync()
        {
            var threshold = DateTime.Now.AddMinutes(-BookingRules.HoldMinutes);
            var staleBookings = await _bookingRepo.GetStaleHoldsAsync(threshold);

            foreach (var booking in staleBookings)
            {
                booking.Status = BookingStatuses.Expired;
            }

            await _bookingRepo.SaveChangesAsync();
            return staleBookings.Count;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _bookingRepo.DeleteAsync(id);
        }
    }
}