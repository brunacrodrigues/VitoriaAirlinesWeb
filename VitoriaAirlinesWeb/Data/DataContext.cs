using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data
{
    /// <summary>
    /// Represents the database context for the Vitoria Airlines web application.
    /// Inherits from IdentityDbContext to manage user authentication and authorization data,
    /// and includes DbSets for all application entities.
    /// </summary>
    public class DataContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Gets or sets the DbSet for managing CustomerProfile entities.
        /// </summary>
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Country entities.
        /// </summary>
        public DbSet<Country> Countries { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Airplane entities.
        /// </summary>
        public DbSet<Airplane> Airplanes { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Seat entities.
        /// </summary>
        public DbSet<Seat> Seats { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Airport entities.
        /// </summary>
        public DbSet<Airport> Airports { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Flight entities.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }


        /// <summary>
        /// Gets or sets the DbSet for managing Ticket entities.
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }



        /// <summary>
        /// Initializes a new instance of the DataContext class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }


        /// <summary>
        /// Configures the model that was discovered by convention from the entity types in the DbSets.
        /// This method is called once when the model is being created.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and foreign keys with DeleteBehavior.Restrict to prevent cascade deletes.
            // This ensures related data is not automatically deleted, requiring manual handling.

            // CustomerProfile to User (One-to-One)
            modelBuilder.Entity<CustomerProfile>()
                .HasOne(cp => cp.User)
                .WithOne(u => u.CustomerProfile)
                .HasForeignKey<CustomerProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CustomerProfile to Country (Many-to-One)
            modelBuilder.Entity<CustomerProfile>()
                .HasOne(cp => cp.Country)
                .WithMany() // No navigation property on Country for CustomerProfile
                .HasForeignKey(cp => cp.CountryId)
                .OnDelete(DeleteBehavior.Restrict);


            // Seat to Airplane (Many-to-One) with Cascade Delete for Seats
            // If an Airplane is deleted, its associated Seats are also deleted.
            modelBuilder.Entity<Seat>()
                .HasOne(seat => seat.Airplane)
                .WithMany(airplane => airplane.Seats)
                .HasForeignKey(seat => seat.AirplaneId)
                .OnDelete(DeleteBehavior.Cascade);


            // Flight to Airplane (Many-to-One)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Airplane)
                .WithMany() // No navigation property on Airplane for Flight (direct collection not needed here)
                .HasForeignKey(f => f.AirplaneId)
                .OnDelete(DeleteBehavior.Restrict);


            // Flight to OriginAirport (Many-to-One)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.OriginAirport)
                .WithMany() // No navigation property on Airport for Origin flights
                .HasForeignKey(f => f.OriginAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            // Flight to DestinationAirport (Many-to-One)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DestinationAirport)
                .WithMany() // No navigation property on Airport for Destination flights
                .HasForeignKey(f => f.DestinationAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            // Ticket to Flight (Many-to-One)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany(f => f.Tickets)
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);


            // Ticket to Seat (Many-to-One)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany() // No navigation property on Seat for Tickets
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);


            // Ticket to User (Many-to-One)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Configure unique indexes to prevent duplicate data for specific properties.

            // CustomerProfile: PassportNumber must be unique
            modelBuilder.Entity<CustomerProfile>()
                .HasIndex(cp => cp.PassportNumber)
                .IsUnique();


            // Country: CountryCode must be unique
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.CountryCode)
                .IsUnique();


            // Airport: IATA code must be unique
            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATA)
                .IsUnique();


            // Seat: Combination of AirplaneId, Row, and Letter must be unique
            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.AirplaneId, s.Row, s.Letter })
                .IsUnique();


            // Ticket: Combination of FlightId and SeatId must be unique (a seat can only be booked once per flight)
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.FlightId, t.SeatId })
                .IsUnique();


            // Flight: FlightNumber must be unique
            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.FlightNumber)
                .IsUnique();

            // Configure enum properties to be stored as strings in the database for readability and flexibility.
            modelBuilder.Entity<Airplane>()
               .Property(f => f.Status)
               .HasConversion<string>();


            modelBuilder.Entity<Seat>()
                .Property(s => s.Class)
                .HasConversion<string>();


            modelBuilder.Entity<Flight>()
                .Property(f => f.Status)
                .HasConversion<string>();

            // Configure decimal properties for currency values to ensure precise storage (10 digits total, 2 after decimal point).
            modelBuilder.Entity<Flight>()
                .Property(f => f.ExecutiveClassPrice)
                .HasPrecision(10, 2);


            modelBuilder.Entity<Flight>()
                .Property(f => f.EconomyClassPrice)
                .HasPrecision(10, 2);


            modelBuilder.Entity<Ticket>()
                .Property(t => t.PricePaid)
                .HasPrecision(10, 2);


        }

    }
}
