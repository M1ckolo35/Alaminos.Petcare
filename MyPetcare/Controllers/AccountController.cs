using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetcare.Data;
using MyPetcare.Models;

namespace MyPetcare.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Index
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Please enter both email and password." });
            }

            // ADMIN ROLE VALIDATION: Enforce @admin.com ONLY for admin role
            if (!string.IsNullOrEmpty(model.Role) && model.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.Email.Trim().EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Admin access requires a valid @admin.com address." });
                }
            }

            // CLIENT ROLE VALIDATION: Enforce @gmail.com ONLY for client role
            if (!string.IsNullOrEmpty(model.Role) && model.Role.Equals("client", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.Email.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Client login requires a valid @gmail.com address." });
                }
            }

            // DATABASE USER CHECK: Verify credentials
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
            if (existingUser == null)
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

            // ROUTING / REDIRECTION BASED ON ROLE
            if (!string.IsNullOrEmpty(model.Role) && model.Role.Equals("client", StringComparison.OrdinalIgnoreCase))
            {
                string clientName = model.Email.Split('@')[0];
                clientName = char.ToUpper(clientName[0]) + clientName.Substring(1);

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("ClientDashboard", "Home", new { name = clientName })
                });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("AdminDashboard", "Home")
                });
            }
        }

        [HttpPost]
        public IActionResult Register([FromBody] RegisterApiDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            // ROLE-BASED EMAIL VALIDATION: Enforce domain based on selected role
            if (!string.IsNullOrEmpty(model.Role) && model.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.Email.Trim().EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Admin registration requires a valid @admin.com address." });
                }
            }
            else
            {
                // Default / Client role validation
                if (!model.Email.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Client registration requires a valid @gmail.com address." });
                }
            }

            // PASSWORD STRENGTH VALIDATION: At least 8 characters, letters, numbers, and special characters
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.Password))
            {
                return Json(new
                {
                    success = false,
                    message = "Password must be at least 8 characters long and contain letters, numbers, and special characters."
                });
            }

            // Check if the email already exists in the database
            if (_context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                return Json(new { success = false, message = "Email is already registered!" });
            }

            try
            {
                var userRole = (!string.IsNullOrEmpty(model.Role) && model.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    ? "Admin"
                    : "Client Account";

                var newUser = new User
                {
                    FullName = string.IsNullOrWhiteSpace(model.Name) ? model.Email.Split('@')[0] : model.Name,
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    Role = userRole
                };

                _context.Users.Add(newUser);
                _context.SaveChanges(); // Saves to Supabase PostgreSQL database

                return Json(new { success = true, message = "Registration successful!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            // Enforce password strength: at least 8 chars, letters, numbers, and special characters
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.NewPassword))
            {
                return Json(new { success = false, message = "Password must be at least 8 characters long and include letters, numbers, and special characters." });
            }

            // Find the user by email in the database
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
            if (user == null)
            {
                return Json(new { success = false, message = "Email address not found." });
            }

            try
            {
                // Update the password
                user.Password = model.NewPassword;
                _context.SaveChanges();

                return Json(new { success = true, message = "Password updated successfully! You can now log in." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend([FromBody] FriendRequestModel model)
        {
            if (string.IsNullOrEmpty(model.UserEmail) || string.IsNullOrEmpty(model.FriendEmail))
                return Json(new { success = false, message = "Invalid email inputs." });

            if (model.UserEmail.ToLower() == model.FriendEmail.ToLower())
                return Json(new { success = false, message = "You cannot add yourself as a friend!" });

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.FriendEmail.ToLower());
            if (targetUser == null)
                return Json(new { success = false, message = "Account not found." });

            var existing = await _context.Friends.FirstOrDefaultAsync(f =>
                (f.UserEmail == model.UserEmail && f.FriendEmail == model.FriendEmail) ||
                (f.UserEmail == model.FriendEmail && f.FriendEmail == model.UserEmail));

            if (existing != null)
                return Json(new { success = false, message = "Already friends or request pending." });

            _context.Friends.Add(new FriendModel { UserEmail = model.UserEmail, FriendEmail = model.FriendEmail, Status = "Accepted" });
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Added {targetUser.FullName ?? targetUser.Email} as a friend!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsList(string userEmail)
        {
            var friendEmails = await _context.Friends
                .Where(f => f.UserEmail == userEmail || f.FriendEmail == userEmail)
                .Select(f => f.UserEmail == userEmail ? f.FriendEmail : f.UserEmail)
                .ToListAsync();

            var friends = await _context.Users
                .Where(u => friendEmails.Contains(u.Email))
                .Select(u => new
                {
                    email = u.Email,
                    name = u.FullName ?? u.Email,
                    avatarJson = u.ProfileAvatarJson
                })
                .ToListAsync();

            return Json(new { success = true, friends });
        }
    }

    // Model for Login
    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // DTO for capturing registration payload from frontend
    public class RegisterApiDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // DTO for Forgot Password requests
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}