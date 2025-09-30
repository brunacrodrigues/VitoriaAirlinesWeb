namespace VitoriaAirlinesAPI.Dtos
{
    public class CustomerProfileDto
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? ProfileImageUrl { get; set; }

        public int? CountryId { get; set; }


        public string? PassportNumber { get; set; }

        public string Email { get; set; } = null!;



    }
}
