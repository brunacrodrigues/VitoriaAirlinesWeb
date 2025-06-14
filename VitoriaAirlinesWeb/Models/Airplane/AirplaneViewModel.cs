using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.Airplane
{
    public class AirplaneViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = null!;


        [Required]
        [Display(Name = "Executive Seats")]
        [Range(0, int.MaxValue)]
        public int TotalExecutiveSeats { get; set; }


        [Required]
        [Display(Name = "Economy Seats")]
        [Range(1, int.MaxValue, ErrorMessage = "Must be greater than 0")]
        public int TotalEconomySeats { get; set; }

        public Guid ImageId { get; set; }


        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
