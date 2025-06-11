using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(100, ErrorMessage = "The field {0} only can contain {1} characters long.")]
        public string FirstName { get; set; } = null!;


        [MaxLength(100, ErrorMessage = "The field {0} only can contain {1} characters long.")]
        public string LastName { get; set; } = null!;


        public Guid? ProfileImageId { get; set; }

        public string ImageFullPath => ProfileImageId == null
            ? "https://brunablob.blob.core.windows.net/images/profilepic.png"
            : $"https://brunablob.blob.core.windows.net/images/{ProfileImageId}";


        public string FullName => $"{FirstName} {LastName}";

    }
}
