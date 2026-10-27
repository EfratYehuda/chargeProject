using DAL.Classes;
using DAL.Data;
using DAL.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.ClassRepositories
{
    public class ChargeBookingRepository : InterfaceChargeBooking
    {
        private readonly AppDbContext _context;

        public ChargeBookingRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<ChargeBooking> Bookings => _context.Set<ChargeBooking>()
            .Include(b => b.Station)
            .AsNoTracking();

        public async Task<ChargeBooking?> GetByIdAsync(int id) =>
            await Bookings.FirstOrDefaultAsync(b => b.Id == id);

        public async Task<List<ChargeBooking>> GetAllAsync() =>
            await Bookings.OrderByDescending(b => b.CreatedAt).ToListAsync();

        public async Task<List<ChargeBooking>> GetByDriverIdAsync(int driverId) =>
            await Bookings.Where(b => b.DriverId == driverId)
                          .OrderByDescending(b => b.CreatedAt).ToListAsync();

        public async Task<List<ChargeBooking>> GetByStationIdAsync(int stationId) =>
            await Bookings.Where(b => b.StationId == stationId)
                          .OrderByDescending(b => b.CreatedAt).ToListAsync();

        public async Task<List<ChargeBooking>> GetByOwnerIdAsync(int ownerId) =>
            await Bookings.Where(b => b.Station.OwnerId == ownerId)
                          .OrderByDescending(b => b.CreatedAt).ToListAsync();

        public async Task<ChargeBooking> AddAsync(ChargeBooking entity)
        {
            _context.Set<ChargeBooking>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ChargeBooking?> UpdateAsync(ChargeBooking entity)
        {
            _context.Set<ChargeBooking>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<ChargeBooking>().FindAsync(id);
            if (entity == null) return false;

            _context.Set<ChargeBooking>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasActiveBookingAsync(int vehicleId, string[] activeStatuses)
        {
            return await _context.Set<ChargeBooking>()
                .AnyAsync(b => b.VehicleId == vehicleId && activeStatuses.Contains(b.Status));
        }

        public async Task<List<ChargeBooking>> GetStaleHoldsAsync(DateTime threshold)
        {
            return await _context.Set<ChargeBooking>()
                .Where(b => b.Status == DTO.BookingStatuses.Held && b.CreatedAt <= threshold)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ChargingStations?> GetStationByIdAsync(int stationId) =>
            await _context.Set<ChargingStations>().AsNoTracking().FirstOrDefaultAsync(s => s.Id == stationId);

        public async Task<UsersVehicles?> GetVehicleByIdAsync(int vehicleId) =>
            await _context.Set<UsersVehicles>().AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
    }
}