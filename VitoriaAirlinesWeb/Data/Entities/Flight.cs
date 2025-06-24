using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Flight : IEntity
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = null!;


        [Required]
        public int AirplaneId { get; set; }
        public Airplane Airplane { get; set; } = null!;


        [Required]
        public int OriginAirportId { get; set; }
        public Airport OriginAirport { get; set; } = null!;


        [Required]
        public int DestinationAirportId { get; set; }
        public Airport DestinationAirport { get; set; } = null!;


        [Required]
        public decimal EconomyClassPrice { get; set; }


        [Required]
        public decimal ExecutiveClassPrice { get; set; }


        [Required]
        public DateTime DepartureUtc { get; set; }


        [Required]
        public TimeSpan Duration { get; set; }


        [Required]
        public FlightStatus Status { get; set; }


        public ICollection<Ticket> Tickets { get; set; }


        public DateTime ArrivalUtc => DepartureUtc.Add(Duration);

        public string FlightInfo =>
                        $"({OriginAirport.IATA}) {OriginAirport.Name} → " +
                        $"({DestinationAirport.IATA}) {DestinationAirport.Name} " +
                        $"on {DepartureUtc.ToLocalTime():dd MMM yyyy HH:mm.}";



    }
}
