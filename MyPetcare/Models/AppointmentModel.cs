using System;

namespace MyPetcare.Models
{
    public class AppointmentModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    }
}
