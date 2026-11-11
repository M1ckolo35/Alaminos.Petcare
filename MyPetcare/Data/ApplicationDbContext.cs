using Microsoft.EntityFrameworkCore;
using MyPetcare.Models;

namespace MyPetcare.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PetHealthRecord> PetHealthRecords { get; set; }
        public DbSet<AppointmentModel> Appointments { get; set; }
        public DbSet<FriendModel> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PetHealthRecord>()
                .Property(p => p.WeightKg)
                .HasPrecision(8, 2);
        }
    }
}