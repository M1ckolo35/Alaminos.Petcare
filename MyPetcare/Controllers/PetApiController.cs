using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetcare.Data;
using MyPetcare.Models;

namespace MyPetcare.Controllers
{
    [Route("api/pets")]
    [ApiController]
    public class PetApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PetApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/pets/appointments?clientName=Michaelradovan66
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments([FromQuery] string? clientName)
        {
            var query = _context.Appointments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                query = query.Where(a => a.ClientName == clientName);
            }

            var appointments = await query
                .OrderByDescending(a => a.Id)
                .Take(100)
                .ToListAsync();

            return Ok(new { success = true, data = appointments });
        }

        // POST /api/pets/create-appointment
        [HttpPost("create-appointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PetName) || string.IsNullOrWhiteSpace(model.Service))
            {
                return BadRequest(new { success = false, message = "Invalid appointment data provided." });
            }

            model.Id = 0;                                   // let PostgreSQL generate the Id
            model.Status = "Pending";
            model.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);

            try
            {
                _context.Appointments.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Appointment successfully created.", id = model.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { success = false, message = "Could not save appointment." });
            }
        }
    }
}