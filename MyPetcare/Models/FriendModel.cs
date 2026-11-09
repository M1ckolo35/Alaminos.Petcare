using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetcare.Models
{
    public class FriendModel
    {
        [Key]
        public int FriendshipId { get; set; }
        [Required]
        public string UserEmail { get; set; } = string.Empty;
        [Required]
        public string FriendEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "Accepted";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
