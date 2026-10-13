using AutoMapper;
using DAL.Classes;
using DTO;
using DTO.REVIEWDTO;
using DTO.STATIONDTO;
using DTO.USERDTO;
using DTO.VEHICLESDTO;

namespace BLL.Mapper
{
    /// <summary>
    /// הגדרות AutoMapper. משמש גם ל-ProjectTo (תרגום ל-SQL), לכן ה-MapFrom כתובים כביטויים שאפשר לתרגם.
    /// שדות שאינם קיימים ב-DTO המקור (Id, Status, CreatedAt...) לא נדרסים בעדכון - הם נקבעים ב-Repository.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ---------- Users ----------
            CreateMap<Users, UserProfileDto>()
                .ForMember(d => d.DriverId, o => o.MapFrom(s => s.DriverProfile != null ? (int?)s.DriverProfile.Id : null))
                .ForMember(d => d.OwnerId, o => o.MapFrom(s => s.OwnerProfile != null ? (int?)s.OwnerProfile.Id : null));
     
            CreateMap<RegisterRequestDto, Users>()
                .ForMember(d => d.Access, o => o.MapFrom(s => s.Role))
                .ForMember(d => d.PasswordHash, o => o.Ignore()); // נקבע ב-Repository מה-hash שהתקבל
            CreateMap<UpdateUserProfileDto, Users>();

            // ---------- Vehicles ----------
            CreateMap<UsersVehicles, VehicleDto>();
            CreateMap<CreateUpdateVehicleDto, UsersVehicles>();

            // ---------- Drivers ----------
            CreateMap<Drivers, DriverDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.User.Name))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.User.Phone))
                .ForMember(d => d.TotalBookings, o => o.MapFrom(s => s.Bookings.Count))
                .ForMember(d => d.Vehicles, o => o.MapFrom(s => s.Vehicles));
            CreateMap<CreateDriverDto, Drivers>();

            // ---------- Owners ----------
            CreateMap<Owner, OwnerDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.User.Name))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.User.Phone))
                .ForMember(d => d.StationsCount, o => o.MapFrom(s => s.Stations.Count));
            CreateMap<CreateOwnerDto, Owner>();

            // ---------- Stations ----------
            CreateMap<ChargingStations, StationDto>()
                .ForMember(d => d.AverageRating, o => o.MapFrom(s =>
                    s.Bookings.Where(b => b.Review != null).Average(b => (double?)b.Review.Rating)))
                .ForMember(d => d.ReviewsCount, o => o.MapFrom(s =>
                    s.Bookings.Count(b => b.Review != null)))
                .ForMember(d => d.CurrentStatus, o => o.MapFrom(s =>
                    s.Logs.OrderByDescending(l => l.UpdatedAt).Select(l => l.Status).FirstOrDefault()));
            CreateMap<CreateUpdateStationDto, ChargingStations>();

            // ---------- Bookings ----------
            CreateMap<ChargeBooking, ChargeBookingDto>()
                .ForMember(d => d.StationName, o => o.MapFrom(s => s.Station.Name))
                .ForMember(d => d.StationAddress, o => o.MapFrom(s => s.Station.Address))
                .ForMember(d => d.VehicleModel, o => o.MapFrom(s => s.Vehicle.Model))
                .ForMember(d => d.HasReview, o => o.MapFrom(s => s.Review != null));
            CreateMap<CreateChargeBookingDto, ChargeBooking>();

            // ---------- Reviews ----------
            CreateMap<ReviewsToCharge, ReviewDto>()
                .ForMember(d => d.StationId, o => o.MapFrom(s => s.Booking.StationId))
                .ForMember(d => d.StationName, o => o.MapFrom(s => s.Booking.Station.Name))
                .ForMember(d => d.DriverName, o => o.MapFrom(s => s.Driver.User.Name));
            CreateMap<CreateReviewDto, ReviewsToCharge>();
            

            // ---------- Station status logs ----------
            CreateMap<StationStatusLogs, StationStatusLogDto>()
                .ForMember(d => d.StationName, o => o.MapFrom(s => s.Station.Name))
                .ForMember(d => d.CurrentDriverName, o => o.MapFrom(s =>
                    s.CurrentDriver != null ? s.CurrentDriver.User.Name : null));
            CreateMap<CreateStationStatusLogDto, StationStatusLogs>();
        }
    }
}
