using AutoMapper;
using BLL.Exceptions;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ChargeBookingService : IChargeBookingService
    {
        private static readonly string[] ActiveStatuses =
            { BookingStatuses.Held, BookingStatuses.Confirmed, BookingStatuses.Charging };

        // Which status may follow which. Anything that is not listed here is forbidden
        // (so Completed / Cancelled / Expired are final, and Held cannot jump straight to Completed).
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            [BookingStatuses.Held] = new[] { BookingStatuses.Confirmed, BookingStatuses.Cancelled },
            [BookingStatuses.Confirmed] = new[] { BookingStatuses.Charging, BookingStatuses.Cancelled },
            [BookingStatuses.Charging] = new[] { BookingStatuses.Completed }
        };

        private readonly InterfaceChargeBooking _bookingRepo;
        private readonly IMapper _mapper;

        public ChargeBookingService(InterfaceChargeBooking bookingRepo, IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<List<ChargeBookingDto>> GetAllAsync()
        {
            var entities = await _bookingRepo.GetAllAsync();
            return _mapper.Map<List<ChargeBookingDto>>(entities);
        }

        public async Task<ChargeBookingDto> GetByIdAsync(int id)
        {
            var entity = await _bookingRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("ההזמנה לא נמצאה");
            return _mapper.Map<ChargeBookingDto>(entity);
        }

        public async Task<ChargeBookingDto> GetOwnedAsync(int id, int driverId)
        {
            var entity = await GetOwnedEntityAsync(id, driverId);
            return _mapper.Map<ChargeBookingDto>(entity);
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

        public async Task<ChargeBookingDto> AddAsync(int driverId, CreateChargeBookingDto dto)
        {
            ValidateBattery(dto);

            var station = await _bookingRepo.GetStationByIdAsync(dto.StationId)
                ?? throw new NotFoundException("העמדה לא נמצאה");
            if (!station.IsActive)
                throw new BusinessRuleException("העמדה אינה פעילה");

            // A vehicle that is not yours looks exactly like a vehicle that does not exist.
            var vehicle = await _bookingRepo.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle == null || vehicle.DriverId != driverId)
                throw new NotFoundException("הרכב לא נמצא");

            if (await _bookingRepo.HasActiveBookingAsync(dto.VehicleId, ActiveStatuses))
                throw new ConflictException("לרכב כבר קיימת הזמנה פעילה");

            var entity = _mapper.Map<ChargeBooking>(dto);
            entity.DriverId = driverId;   // from the token, not from the client
            entity.Status = BookingStatuses.Held;
            entity.CreatedAt = DateTime.Now;

            var savedEntity = await _bookingRepo.AddAsync(entity);

            // Re-read so the station name/address are loaded for the DTO.
            var fullEntity = await _bookingRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<ChargeBookingDto>(fullEntity ?? savedEntity);
        }

        // IDOR protection: the booking is loaded only if it belongs to this driver.
        public async Task<ChargeBookingDto> UpdateAsync(int id, int driverId, CreateChargeBookingDto dto)
        {
            ValidateBattery(dto);

            var entity = await GetOwnedEntityAsync(id, driverId);

            if (entity.Status != BookingStatuses.Held && entity.Status != BookingStatuses.Confirmed)
                throw new ConflictException("ניתן לעדכן הזמנה רק לפני תחילת הטעינה");

            entity.CurrentBatteryPercent = dto.CurrentBatteryPercent;
            entity.TargetBatteryPercent = dto.TargetBatteryPercent;
            entity.EstimatedArrivalTime = dto.EstimatedArrivalTime;

            var updatedEntity = await _bookingRepo.UpdateAsync(entity);
            return _mapper.Map<ChargeBookingDto>(updatedEntity);
        }

        public async Task<ChargeBookingDto> UpdateStatusAsync(int id, int driverId, string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !Regex.IsMatch(status, BookingStatuses.Pattern))
                throw new BusinessRuleException("סטטוס לא חוקי");

            var entity = await GetOwnedEntityAsync(id, driverId);

            if (!AllowedTransitions.TryGetValue(entity.Status, out var allowed) || !allowed.Contains(status))
                throw new ConflictException($"לא ניתן לעבור מסטטוס {entity.Status} לסטטוס {status}");

            // A hold that is older than HoldMinutes cannot be confirmed any more.
            if (entity.Status == BookingStatuses.Held && status == BookingStatuses.Confirmed
                && entity.CreatedAt <= DateTime.Now.AddMinutes(-BookingRules.HoldMinutes))
                throw new ConflictException("השריון פג תוקף");

            entity.Status = status;
            var updatedEntity = await _bookingRepo.UpdateAsync(entity);
            return _mapper.Map<ChargeBookingDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id, int driverId)
        {
            await GetOwnedEntityAsync(id, driverId);   // throws NotFoundException if it is not his
            await _bookingRepo.DeleteAsync(id);
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

        private async Task<ChargeBooking> GetOwnedEntityAsync(int id, int driverId) =>
            await _bookingRepo.GetOwnedAsync(id, driverId)
                ?? throw new NotFoundException("ההזמנה לא נמצאה");

        private static void ValidateBattery(CreateChargeBookingDto dto)
        {
            if (dto.TargetBatteryPercent <= dto.CurrentBatteryPercent)
                throw new BusinessRuleException("אחוז הסוללה המבוקש חייב להיות גבוה מהנוכחי");
        }
    }
}