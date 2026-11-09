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

        // Inject ApplicationDbContext so data persists directly into SQL Server
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult ClientDashboard(string name = "Pet Owner")
        {
            // 1. Assign to a strict string variable (Prevents CS1963 dynamic error)
            string clientName = string.IsNullOrEmpty(name) ? "Emelynradovan" : name;
            ViewBag.ClientName = clientName;
            // 2. Find the user's email from the database safely
            var matchedUser = _context.Users.FirstOrDefault(u => u.Email.Contains(clientName));
            ViewBag.UserEmail = matchedUser?.Email ?? $"{clientName.ToLowerInvariant()}@gmail.com";

            // 3. Query SQL Server specifically for this logged-in user's appointments
            ViewBag.Appointments = _context.Appointments
                .Where(a => a.ClientName == clientName)
                .OrderByDescending(a => a.Id)
                .ToList();

            return View("~/Views/ClientDashboard/ClientDashboard.cshtml");
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            // Fetch all appointments from SQL Server
            ViewBag.Appointments = _context.Appointments
                .OrderByDescending(a => a.Id)
                .ToList();

            // Fetch all registered users from SQL Server and pass them as the Model
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

            // 3. Save the new appointment directly to your SQL Server database
            _context.Appointments.Add(model);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Appointment successfully recorded in SQL Server!",
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

        // Endpoint called by Admin to Approve or Reject
        [HttpPost]
        public IActionResult UpdateAppointmentStatus([FromBody] StatusUpdateModel model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid data." });

            var appointment = _context.Appointments.Find(model.Id);
            if (appointment != null)
            {
                appointment.Status = model.Status; // "Confirmed" or "Rejected"
                _context.SaveChanges(); // Save the status update to SQL Server

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