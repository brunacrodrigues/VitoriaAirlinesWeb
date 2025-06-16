using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Airplane> Airplanes { get; set; }

        public DbSet<Seat> Seats { get; set; }

        public DbSet<Airport> Airports { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerProfile>()
                .HasOne(cp => cp.User)
                .WithOne(u => u.CustomerProfile)
                .HasForeignKey<CustomerProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seat>()
                .HasOne(seat => seat.Airplane)
                .WithMany(airplane => airplane.Seats)
                .HasForeignKey(seat => seat.AirplaneId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CustomerProfile>()
                .HasIndex(cp => cp.PassportNumber)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.CountryCode)
                .IsUnique();

            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATA)
                .IsUnique();

            modelBuilder.Entity<Seat>()
                .HasIndex(seat => new { seat.AirplaneId, seat.Row, seat.Letter })
                .IsUnique();

            modelBuilder.Entity<Seat>()
                .Property(s => s.Class)
                .HasConversion<string>();

        }

    }
}
