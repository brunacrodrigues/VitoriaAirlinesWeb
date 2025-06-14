using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Airplane : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = null!;


        [Display(Name = "Executive Seats")]
        [Range(0, int.MaxValue)]
        public int TotalExecutiveSeats { get; set; }


        [Display(Name = "Economy Seats")]
        [Range(0, int.MaxValue)]
        public int TotalEconomySeats { get; set; }


        [Required]
        public Guid ImageId { get; set; }


        public ICollection<Seat> Seats { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
        ? $"https://brunablob.blob.core.windows.net/images/noimage.png"
        : $"https://brunablob.blob.core.windows.net/images/{ImageId}";
    }
}
