using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a seat configuration within an airplane.
    /// Includes details about its position (row and letter) and class (Executive or Economy).
    /// </summary>
    public class Seat : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the seat.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the row number of the seat.
        /// </summary>

        public int Row { get; set; }


        /// <summary>
        /// Gets or sets the letter identifier of the seat within its row (e.g., "A", "B").
        /// </summary>
        public string Letter { get; set; } = null!;


        /// <summary>
        /// Gets or sets the class of the seat (e.g., Economy, Executive).
        /// </summary>
        public SeatClass Class { get; set; }


        /// <summary>
        /// Gets or sets the foreign key to the associated Airplane.
        /// </summary>
        public int AirplaneId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Airplane entity.
        /// </summary>
        public Airplane Airplane { get; set; } = null!;

    }
}
