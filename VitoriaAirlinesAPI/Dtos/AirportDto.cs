namespace VitoriaAirlinesAPI.Dtos
{
    public class AirportDto
    {
        public int Id { get; set; }
        public string IATA { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty; // e.g., "PT"
        public string City { get; set; } = string.Empty;
    }
}
