using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services;
using System.Security.Claims;
namespace MotshwaneConsortiumGroup.Controllers;
public class CustomerController : Controller
{
    private readonly DemoDataService _data;
    public CustomerController(DemoDataService data) => _data = data;
    [Authorize(Roles = "Customer")]
    public IActionResult Dashboard() => View(_data.Bookings);
    [HttpGet] public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Register(string name, string phone, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
        { ModelState.AddModelError("", "Please complete all fields. Password must be at least 8 characters."); return View(); }
        TempData["Message"] = "Registration successful. You can sign in";
        return RedirectToAction(nameof(Login));
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        string? role = null;
        string? displayName = null;

       
        // Replaced with database-backed authentication later.
        if (email.Equals("admin@motshwane.co.za",
        StringComparison.OrdinalIgnoreCase) &&
        password == "Admin123!")
        {
            role = "Admin";
            displayName = "Motshwane Admin";
        }
        else if (email.Equals("staff@motshwane.co.za",
        StringComparison.OrdinalIgnoreCase) &&
        password == "Staff123!")
        {
            role = "Staff";
            displayName = "Motshwane Staff";
        }
        else if (email.Equals("customer@motshwane.co.za",
        StringComparison.OrdinalIgnoreCase) &&
        password == "Customer123!")
        {
            role = "Customer";
            displayName = "Customer";
        }

        if (role == null)
        {
            ModelState.AddModelError("", "Invalid email address or password.");
            return View();
        }

        var claims = new List<Claim>
{
new Claim(ClaimTypes.Name, displayName),
new Claim(ClaimTypes.Email, email),
new Claim(ClaimTypes.Role, role)
};

        var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal);

        return role switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Staff" => RedirectToAction("Dashboard", "Staff"),
            _ => RedirectToAction("Dashboard", "Customer")
        };
    }


    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }



    [HttpGet] public IActionResult Booking() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Booking(string name, string service, DateTime date, string location, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(location) || date.Date < DateTime.Today)
        { ModelState.AddModelError("", "Please complete all required fields and choose a valid date."); return View(); }
        TempData["BookingReference"] = "MC-" + Random.Shared.Next(100,999);
        TempData["BookingService"] = service;
        return RedirectToAction(nameof(Confirmation));
    }
    [HttpGet] public IActionResult UploadProof() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult UploadProof(string reference, IFormFile? proof)
    {
        if (string.IsNullOrWhiteSpace(reference) || proof is null || proof.Length == 0)
        { ModelState.AddModelError("", "Please enter a booking reference and choose a file."); return View(); }
        TempData["Message"] = "Proof of payment submitted successfully";
        return RedirectToAction(nameof(MyBookings));
    }
    public IActionResult Confirmation() => View();
    public IActionResult MyBookings() => View(_data.Bookings);
}
