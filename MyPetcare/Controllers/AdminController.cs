using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetcare.Data;
using MyPetcare.Models;

namespace MyPetcare.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Kunin ang appointments at i-sort: Newest / Current date ang nauuna sa itaas
            var appointments = await _context.Appointments
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.Appointments = appointments;

            // 2. Kunin ang health records
            var healthRecords = await _context.PetHealthRecords.ToListAsync();
            ViewBag.HealthRecords = healthRecords;

            // Kung kailangan mo rin ipasa ang Users list sa Model (kung naka-Model IEnumerable<User> ang view mo):
            var users = await _context.Users.ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHealthRecord([FromBody] PetHealthRecord model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.PetName))
                {
                    return Json(new { success = false, message = "Pet Name is required." });
                }

                // Hanapin ang pet base sa pangalan
                var existing = await _context.PetHealthRecords
                    .FirstOrDefaultAsync(r => r.PetName.ToLower() == model.PetName.ToLower());

                if (existing != null)
                {
                    // Kung meron na, i-update ang mga field
                    existing.Diagnosis = model.Diagnosis;
                    existing.VaccinesGiven = model.VaccinesGiven;
                    existing.WeightKg = model.WeightKg;
                    existing.TreatmentNotes = model.TreatmentNotes;
                    existing.LastUpdated = DateTime.Now;
                }
                else
                {
                    // Kung wala pa, magdagdag ng bagong record
                    model.LastUpdated = DateTime.Now;
                    _context.PetHealthRecords.Add(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Pet health record saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}