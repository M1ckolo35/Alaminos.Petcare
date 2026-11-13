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

            string email = model.Email.Trim();

            // DATABASE USER CHECK: Verify credentials securely from backend database
            var existingUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Password == model.Password);
            if (existingUser == null)
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

            // SECURE ROLE DETECTION: Determined strictly by database record or email suffix
            bool isAdmin = email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase) ||
                           (!string.IsNullOrEmpty(existingUser.Role) && existingUser.Role.Contains("Admin", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("AdminDashboard", "Home")
                });
            }
            else
            {
                string clientName = email.Split('@')[0];
                clientName = char.ToUpper(clientName[0]) + clientName.Substring(1);

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("ClientDashboard", "Home", new { name = clientName })
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

            string email = model.Email.Trim();

            if (email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Admin accounts cannot be registered publicly." });
            }

            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.Password))
            {
                return Json(new
                {
                    success = false,
                    message = "Password must be at least 8 characters long and contain letters, numbers, and special characters."
                });
            }

            if (_context.Users.Any(u => u.Email.ToLower() == email.ToLower()))
            {
                return Json(new { success = false, message = "Email is already registered!" });
            }

            try
            {
                var newUser = new User
                {
                    FullName = string.IsNullOrWhiteSpace(model.Name) ? email.Split('@')[0] : model.Name,
                    Email = email,
                    Password = model.Password,
                    Role = "Client Account"
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return Json(new { success = true, message = "Client registration successful! You can now log in." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.GetBaseException().Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
            if (!passwordRegex.IsMatch(model.NewPassword))
            {
                return Json(new { success = false, message = "Password must be at least 8 characters long and include letters, numbers, and special characters." });
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

                if (user == null)
                {
                    return Json(new { success = false, message = "Email address not found." });
                }

                user.Password = model.NewPassword;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Password updated successfully! You can now log in." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.GetBaseException().Message });
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

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterApiDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}