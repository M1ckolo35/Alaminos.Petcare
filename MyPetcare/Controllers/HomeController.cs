using Microsoft.AspNetCore.Mvc;
using MyPetcare.Data;
using MyPetcare.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPetcare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ClientDashboard(string name = "Pet Owner")
        {
            string clientName = string.IsNullOrEmpty(name) ? "Emelynradovan" : name;
            ViewBag.ClientName = clientName;

            // PostgreSQL is case-sensitive, so compare in lowercase
            string lowerName = clientName.ToLowerInvariant();
            var matchedUser = _context.Users.FirstOrDefault(u => u.Email.ToLower().Contains(lowerName));
            ViewBag.UserEmail = matchedUser?.Email ?? $"{lowerName}@gmail.com";

            ViewBag.Appointments = _context.Appointments
                .Where(a => a.ClientName == clientName)
                .OrderByDescending(a => a.Id)
                .ToList();

            return View("~/Views/ClientDashboard/ClientDashboard.cshtml");
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            ViewBag.Appointments = _context.Appointments
                .OrderByDescending(a => a.Id)
                .ToList();

            var usersList = _context.Users.ToList();

            return View("~/Views/Admin/Dashboard.cshtml", usersList);
        }

        [HttpPost]
        public IActionResult CreateAppointment([FromBody] AppointmentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PetName) || string.IsNullOrWhiteSpace(model.Service))
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            model.Status = "Pending";

            // Npgsql only accepts UTC DateTime for timestamptz columns.
            // SpecifyKind keeps the exact time the user picked and labels it UTC.
            model.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);

            try
            {
                _context.Appointments.Add(model);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // visible in Render's Logs tab
                return Json(new
                {
                    success = false,
                    message = "Could not save appointment: " + ex.GetBaseException().Message
                });
            }

            return Json(new
            {
                success = true,
                message = "Appointment successfully recorded!",
                appointment = new
                {
                    id = model.Id,
                    clientName = model.ClientName,
                    petName = model.PetName,
                    service = model.Service,
                    dateFormatted = model.AppointmentDate.ToString("MMM dd, yyyy - hh:mm tt"),
                    status = model.Status
                }
            });
        }

        [HttpPost]
        public IActionResult UpdateAppointmentStatus([FromBody] StatusUpdateModel model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid data." });

            var appointment = _context.Appointments.Find(model.Id);
            if (appointment != null)
            {
                appointment.Status = model.Status; // "Confirmed" or "Rejected"
                _context.SaveChanges();

                return Json(new { success = true, message = $"Appointment status updated to {model.Status}." });
            }
            return Json(new { success = false, message = "Appointment not found." });
        }
    }

    public class StatusUpdateModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}