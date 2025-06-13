using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Country : IEntity
    {
        public int Id { get; set ; }


        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = null!;


        [MaxLength(2)]
        [Required]
        public string CountryCode { get; set; } = null!;


        public string FlagImageUrl => $"https://flagcdn.com/w40/{CountryCode.ToLower()}.png";
    }
}
