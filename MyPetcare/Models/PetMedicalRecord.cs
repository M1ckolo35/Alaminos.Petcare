using System.ComponentModel.DataAnnotations;

namespace MyPetcare.Models
{
    public class PetHealthRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PetName { get; set; } = string.Empty;

        public string SpeciesBreed { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string VaccinesGiven { get; set; } = string.Empty;

        public decimal WeightKg { get; set; }

        public string TreatmentNotes { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public string VeterinarianName { get; set; } = "Dr. Admin";
    }
}