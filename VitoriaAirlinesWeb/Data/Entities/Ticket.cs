using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a ticket entity for a flight in the Vitoria Airlines system.
    /// Includes details about the flight, seat, user, price, purchase date, and cancellation status.
    /// </summary>
    public class Ticket : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the ticket.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the foreign key to the associated Flight. This field is required.
        /// </summary>
        [Required]
        public int FlightId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Flight entity. This field is required.
        /// </summary>
        public Flight Flight { get; set; }


        /// <summary>
        /// Gets or sets the foreign key to the associated Seat. This field is required.
        /// </summary>
        [Required]
        public int SeatId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Seat entity. This field is required.
        /// </summary>
        public Seat Seat { get; set; }


        /// <summary>
        /// Gets or sets the foreign key (string GUID) to the associated User. This field is required.
        /// </summary>
        [Required]
        public string UserId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated User entity. This field is required.
        /// </summary>
        public User User { get; set; }


        /// <summary>
        /// Gets or sets the price paid for this ticket. This field is required.
        /// </summary>
        [Required]
        public decimal PricePaid { get; set; }


        /// <summary>
        /// Gets or sets the purchase date and time of the ticket in Coordinated Universal Time (UTC). This field is required.
        /// </summary>
        [Required]
        public DateTime PurchaseDateUtc { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the ticket has been canceled. Default is false.
        /// </summary>
        public bool IsCanceled { get; set; } = false;


        /// <summary>
        /// Gets or sets the date and time in UTC when the ticket was canceled. This property is nullable.
        /// </summary>
        public DateTime? CanceledDateUtc { get; set; }
    }
}
