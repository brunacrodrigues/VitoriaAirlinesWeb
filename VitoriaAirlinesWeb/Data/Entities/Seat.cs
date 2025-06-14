using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Seat : IEntity
    {
        public int Id { get; set; }

        public int Row { get; set; }

        public string Letter { get; set; } = null!;

        public SeatClass Class { get; set; }

        public int AirplaneId { get; set; }
        public Airplane Airplane { get; set; } = null!;

    }
}
