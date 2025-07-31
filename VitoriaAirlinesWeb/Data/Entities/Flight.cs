using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a flight entity in the Vitoria Airlines system.
    /// Includes details about its number, associated airplane, origin and destination airports, pricing, schedule, and status.
    /// </summary>
    public class Flight : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the flight.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the unique flight number. This field is required and has a maximum length of 10 characters.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets the foreign key to the associated Airplane. This field is required.
        /// </summary>
        [Required]
        public int AirplaneId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Airplane entity. This field is required.
        /// </summary>        
        public Airplane Airplane { get; set; } = null!;


        /// <summary>
        /// Gets or sets the foreign key to the origin Airport. This field is required.
        /// </summary>
        [Required]
        public int OriginAirportId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the origin Airport entity. This field is required.
        /// </summary>
        public Airport OriginAirport { get; set; } = null!;


        /// <summary>
        /// Gets or sets the foreign key to the destination Airport. This field is required.
        /// </summary>
        [Required]
        public int DestinationAirportId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the destination Airport entity. This field is required.
        /// </summary>
        public Airport DestinationAirport { get; set; } = null!;


        /// <summary>
        /// Gets or sets the price for economy class tickets on this flight. This field is required.
        /// </summary>
        [Required]
        public decimal EconomyClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the price for executive class tickets on this flight. This field is required.
        /// </summary>
        [Required]
        public decimal ExecutiveClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the departure date and time in Coordinated Universal Time (UTC). This field is required.
        /// </summary>
        [Required]
        public DateTime DepartureUtc { get; set; }


        /// <summary>
        /// Gets or sets the duration of the flight. This field is required.
        /// </summary>
        [Required]
        public TimeSpan Duration { get; set; }


        /// <summary>
        /// Gets or sets the current status of the flight (e.g., Scheduled, Departed, Arrived, Canceled). This field is required.
        /// </summary>
        [Required]
        public FlightStatus Status { get; set; }


        /// <summary>
        /// Gets or sets the collection of tickets associated with this flight.
        /// </summary>
        public ICollection<Ticket> Tickets { get; set; }


        /// <summary>
        /// Gets the calculated arrival date and time in UTC, derived from DepartureUtc and Duration.
        /// </summary>
        public DateTime ArrivalUtc => DepartureUtc.Add(Duration);


        /// <summary>
        /// Gets a formatted string containing key flight information for display.
        /// Includes IATA codes, airport names, and local departure time.
        /// </summary>
        public string FlightInfo =>
                        $"({OriginAirport.IATA}) {OriginAirport.Name} → " +
                        $"({DestinationAirport.IATA}) {DestinationAirport.Name} " +
                        $"on {DepartureUtc.ToLocalTime():dd MMM yyyy HH:mm.}";



    }
}
