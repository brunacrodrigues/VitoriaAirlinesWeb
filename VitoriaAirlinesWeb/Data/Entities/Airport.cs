using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Airport : IEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string IATA { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required]
        public int CountryId { get; set; }

        public Country Country { get; set; } = null!;


    }
}
