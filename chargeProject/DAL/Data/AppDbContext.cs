using DAL.Classes;
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

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Avoid SQL Server "multiple cascade paths" errors
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                                                 .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
            // הגבלת אורך לסטטוס - חובה כדי שהמסד יוכל לסנן לפיו באינדקס
            modelBuilder.Entity<ChargeBooking>()
                .Property(b => b.Status)
                .HasMaxLength(50);

            // חוק סנכרון: רק הזמנה פעילה אחת לכל תחנה
            modelBuilder.Entity<ChargeBooking>()
                .HasIndex(b => b.StationId).IsUnique()
                .HasFilter("[Status] IN ('Held', 'Confirmed', 'Charging')")
                .HasDatabaseName("IX_OneActiveBookingPerStation");
        }

    }

}