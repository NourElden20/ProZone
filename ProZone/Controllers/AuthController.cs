using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProZone.Models;
using ProZone.Models.VMs;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProZone.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMongoCollection<ApplicationUser> _mongoCollection;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AuthController(UserManager<ApplicationUser> userManager, MongoDbService mongoDbService, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _mongoCollection = mongoDbService.Database.GetCollection<ApplicationUser>("users");
            _signInManager = signInManager;
        }

        

        // GET: AuthController/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: AuthController/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel vm)
        {
            var User = new ApplicationUser()
            {
                UserName = vm.UserName,
                Email = vm.Email,
                role = "User",
                SecurityStamp = Guid.NewGuid().ToString() // Manually assign a security stamp
            };
            var result = await _userManager.CreateAsync(User, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View();
            }
            await _signInManager.SignInAsync(User, isPersistent:false);
            return RedirectToAction("Index","Home");

        }
        // GET: AuthController/Login
        public ActionResult Login()
        {
            return View();
        }
        // POST: AuthController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel vm)
        {
            if(!ModelState.IsValid)
                return View();
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email not found");
                return View(); // Return back to login page if user not found
            }
            // Try to sign in the user with the provided credentials
            var result = await _signInManager.PasswordSignInAsync(user, vm.Password,true,false);

            if (result.Succeeded)
            {
                // Add the user ID to the cookie
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7), // Set expiry as needed
                    HttpOnly = true, // Make it inaccessible via JavaScript (for security)
                    Secure = true, // Use only over HTTPS (optional, but recommended)
                    SameSite = SameSiteMode.Strict // Ensures the cookie is sent only on the same site
                };

                // Store user ID in the cookie
                HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                // Redirect to the desired page after successful login
                return RedirectToAction("Index", "Home"); // Redirect after successful login
            }

            // Handle failure case
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(); // Return back to login page with error

        }
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var viewModel = new ProfileViewModel
            {
                Email = user.Email,
                Name = user.UserName,
                JoinDate = user.CreatedOn.Date.ToShortDateString(),
                Role = user.role
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }


    }
}
