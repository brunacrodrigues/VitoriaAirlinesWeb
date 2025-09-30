using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesAPI.Dtos
{
    public class CompleteBookingRequestDto
    {
        [Required]
        public string StripeSessionId { get; set; }
    }
}
