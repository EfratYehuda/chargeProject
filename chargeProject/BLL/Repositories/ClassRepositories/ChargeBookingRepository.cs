using BLL.Repositories.IRepositories;
using DAL.Classes;
using DAL.Data;
using DTO.VEHICLESDTO;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using DTO;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repositories.ClassRepositories
{
    /// <summary>מימוש גישה להזמנות טעינה ושריונים</summary>
    public class ChargeBookingRepository : InterfaceChargeBooking
    {
        private static readonly string[] ActiveStatuses =
            { BookingStatuses.Held, BookingStatuses.Confirmed, BookingStatuses.Charging };

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ChargeBookingRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<ChargeBooking> Bookings => _context.Set<ChargeBooking>().AsNoTracking();

        public async Task<ChargeBookingDto?> GetByIdAsync(int id) =>
            await Bookings.Where(b => b.Id == id)
                .ProjectTo<ChargeBookingDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        public async Task<List<ChargeBookingDto>> GetAllAsync() =>
            await Bookings.OrderByDescending(b => b.CreatedAt)
                .ProjectTo<ChargeBookingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ChargeBookingDto>> GetByDriverIdAsync(int driverId) =>
            await Bookings.Where(b => b.DriverId == driverId)
                .OrderByDescending(b => b.CreatedAt)
                .ProjectTo<ChargeBookingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ChargeBookingDto>> GetByStationIdAsync(int stationId) =>
            await Bookings.Where(b => b.StationId == stationId)
                .OrderByDescending(b => b.CreatedAt)
                .ProjectTo<ChargeBookingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<List<ChargeBookingDto>> GetByOwnerIdAsync(int ownerId) =>
            await Bookings.Where(b => b.Station.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAt)
                .ProjectTo<ChargeBookingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<ChargeBookingDto> AddAsync(CreateChargeBookingDto dto)
        {
            var station = await _context.Set<ChargingStations>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.StationId)
                ?? throw new KeyNotFoundException($"עמדה {dto.StationId} לא נמצאה");
            if (!station.IsActive)
                throw new InvalidOperationException("העמדה אינה פעילה");

            var vehicle = await _context.Set<UsersVehicles>().AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId)
                ?? throw new KeyNotFoundException($"רכב {dto.VehicleId} לא נמצא");
            if (vehicle.DriverId != dto.DriverId)
                throw new InvalidOperationException("הרכב אינו שייך לנהג");

            if (await _context.Set<ChargeBooking>().AnyAsync(b =>
                    b.VehicleId == dto.VehicleId && ActiveStatuses.Contains(b.Status)))
                throw new InvalidOperationException("לרכב כבר קיימת הזמנה פעילה");

            var entity = _mapper.Map<ChargeBooking>(dto);
            entity.Status = BookingStatuses.Held;
            entity.CreatedAt = DateTime.Now;

            _context.Set<ChargeBooking>().Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<ChargeBookingDto?> UpdateAsync(int id, CreateChargeBookingDto dto)
        {
            var entity = await _context.Set<ChargeBooking>().FindAsync(id);
            if (entity == null) return null;

            // מעדכנים רק נתוני טעינה וזמן הגעה; עמדה/נהג/רכב/סטטוס/CreatedAt נשארים
            entity.CurrentBatteryPercent = dto.CurrentBatteryPercent;
            entity.TargetBatteryPercent = dto.TargetBatteryPercent;
            entity.EstimatedArrivalTime = dto.EstimatedArrivalTime;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<ChargeBookingDto?> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Set<ChargeBooking>().FindAsync(id);
            if (entity == null) return null;

            entity.Status = status;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<int> ExpireStaleHoldsAsync()
        {
            var threshold = DateTime.Now.AddMinutes(-BookingRules.HoldMinutes);
            var stale = await _context.Set<ChargeBooking>()
                .Where(b => b.Status == BookingStatuses.Held && b.CreatedAt <= threshold)
                .ToListAsync();

            foreach (var booking in stale)
                booking.Status = BookingStatuses.Expired;

            await _context.SaveChangesAsync();
            return stale.Count;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ChargeBooking>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ChargeBooking>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
