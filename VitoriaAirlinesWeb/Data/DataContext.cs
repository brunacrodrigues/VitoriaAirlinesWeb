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

        public DbSet<Flight> Flights { get; set; }

        public DbSet<Ticket> Tickets { get; set; }



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

            modelBuilder.Entity<CustomerProfile>()
                .HasOne(cp => cp.Country)
                .WithMany()
                .HasForeignKey(cp => cp.CountryId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Seat>()
                .HasOne(seat => seat.Airplane)
                .WithMany(airplane => airplane.Seats)
                .HasForeignKey(seat => seat.AirplaneId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Airplane)
                .WithMany()
                .HasForeignKey(f => f.AirplaneId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Flight>()
                .HasOne(f => f.OriginAirport)
                .WithMany()
                .HasForeignKey(f => f.OriginAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DestinationAirport)
                .WithMany()
                .HasForeignKey(f => f.DestinationAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany(f => f.Tickets)
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);


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
                .HasIndex(s => new { s.AirplaneId, s.Row, s.Letter })
                .IsUnique();


            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.FlightId, t.SeatId })
                .IsUnique();


            modelBuilder.Entity<Airplane>()
               .Property(f => f.Status)
               .HasConversion<string>();


            modelBuilder.Entity<Seat>()
                .Property(s => s.Class)
                .HasConversion<string>();


            modelBuilder.Entity<Flight>()
                .Property(f => f.Status)
                .HasConversion<string>();


            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.FlightNumber)
                .IsUnique();

            modelBuilder.Entity<Flight>()
                .Property(f => f.ExecutiveClassPrice)
                .HasPrecision(10, 2); // mais moderno que o HasColumnType


            modelBuilder.Entity<Flight>()
                .Property(f => f.EconomyClassPrice)
                .HasPrecision(10, 2);


            modelBuilder.Entity<Ticket>()
                .Property(t => t.PricePaid)
                .HasPrecision(10, 2);


        }

    }
}
