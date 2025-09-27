using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesAPI.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
