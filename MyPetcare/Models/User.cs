using System.ComponentModel.DataAnnotations;

namespace MyPetcare.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        [Required]
        public string Role { get; set; } = "Client";

        // NEW Profile Customization Fields
        public string ProfilePictureUrl { get; set; } = "/images/avatars/default-avatar.png";
        public string Bio { get; set; } = "Proud pet parent!";
        public string PreferredTheme { get; set; } = "dark";
        public string? ProfileAvatarJson { get; set; } 
    }
}