using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<ChargeBooking> ChargeBookings { get; set; }
        public DbSet<ChargingStations> ChargingStations { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<ReviewsToCharge> ReviewsToCharges { get; set; }
        public DbSet<StationStatusLogs> StationStatusLogs { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersVehicles> UsersVehicles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Avoid SQL Server "multiple cascade paths" errors
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                                                 .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }

}