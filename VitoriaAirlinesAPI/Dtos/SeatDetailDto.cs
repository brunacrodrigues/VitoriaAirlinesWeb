using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesAPI.Dtos
{
    public class SeatDetailDto
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public string Letter { get; set; } = null!;
        public SeatClass Class { get; set; }
        public bool IsOccupied { get; set; }
    }
}
