using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesAPI.Dtos;

public class UpdateCustomerProfileDto
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public int? CountryId { get; set; }

    public string? PassportNumber { get; set; }

    public bool RemoveImage { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
