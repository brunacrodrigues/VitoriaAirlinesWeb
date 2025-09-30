using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data
{
    public class BookingRequestDto
    {
        [Required]
        public List<BookingLegDto> Legs { get; set; } = new List<BookingLegDto>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }


        [EmailAddress]
        public string? Email { get; set; }

        public string? PassportNumber { get; set; }
    }
}
