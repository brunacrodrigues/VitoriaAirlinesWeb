using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesAPI.Dtos
{
    public class RegisterRequestDto
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
