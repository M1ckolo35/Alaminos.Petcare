using Microsoft.AspNetCore.Mvc;
using MyPetcare.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyPetcare.Controllers
{
    // Route prefix for all API endpoints in this controller: /api/pets
    [Route("api/pets")]
    [ApiController]
    public class PetApiController : ControllerBase
    {
        // -------------------------------------------------------------
        // GET ENDPOINT: Retrieve all client appointments
        // URL: GET /api/pets/appointments
        // -------------------------------------------------------------
        [HttpGet("appointments")]
        public IActionResult GetAppointments()
        {
            // Fetch records from the database using Entity Framework Core (e.g., _context.Appointments.ToList())
            var appointments = new List<AppointmentModel>
            {
                new AppointmentModel { PetName = "Max (Corgi)", Service = "Vaccination", Status = "Confirmed", AppointmentDate = System.DateTime.Now }
            };

            // Return success response with data in JSON format
            return Ok(new { success = true, data = appointments });
        }

        // -------------------------------------------------------------
        // POST ENDPOINT: Create and save a new appointment from the frontend
        // URL: POST /api/pets/create-appointment
        // -------------------------------------------------------------
        [HttpPost("create-appointment")]
        public IActionResult CreateAppointment([FromBody] AppointmentModel model)
        {
            // Validate incoming payload
            if (model == null || string.IsNullOrEmpty(model.PetName))
            {
                return BadRequest(new { success = false, message = "Invalid appointment data provided." });
            }

            // Save the appointment to the database using Entity Framework Core
            // Example:
            // _context.Appointments.Add(model);
            // _context.SaveChanges();

            // Return success confirmation response
            return Ok(new { success = true, message = "Appointment successfully created via API!" });
        }
    }
}
