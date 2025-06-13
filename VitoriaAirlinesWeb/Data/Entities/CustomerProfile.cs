using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class CustomerProfile : IEntity
    {
        public int Id { get; set; }


        public int? CountryId { get; set; }


        public Country? Country { get; set; }


        [MaxLength(20)]
        public string? PassportNumber { get; set; }


        [Required]
        public string UserId { get; set; } = null!;


        public User User { get; set; } = null!;



    }
}
