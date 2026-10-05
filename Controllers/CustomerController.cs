using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Controllers;

public class CustomerController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    private readonly IPaymentService _payments;
    private readonly IFileStorageService _fileStorage;
    // Auth (Register/Login/Logout) talks to the database directly rather than through a
    // dedicated service interface — it's account/identity logic, not booking business logic,
    // so it doesn't need the same swappable-implementation treatment the other services do.
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public CustomerController(IBookingService bookings, ICatalogService catalog, IPaymentService payments,
        IFileStorageService fileStorage, ApplicationDbContext context)
    {
        _bookings = bookings;
        _catalog = catalog;
        _payments = payments;
        _fileStorage = fileStorage;
        _context = context;
    }

    private int CurrentCustomerId => int.Parse(User.FindFirstValue("CustomerId")!);
    private bool IsLoggedIn => User.Identity?.IsAuthenticated ?? false;

    // ---------- Auth ----------

    [HttpGet] public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string phone, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        { ModelState.AddModelError("", "Please complete all fields."); return View(); }

        if (password.Length < 8)
        { ModelState.AddModelError("", "Password must be at least 8 characters."); return View(); }

        email = email.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Email == email))
        { ModelState.AddModelError("", "An account with this email already exists."); return View(); }

        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
        if (customerRole is null)
        { ModelState.AddModelError("", "Customer role was not found in the database."); return View(); }

        var user = new User { FullName = name.Trim(), Email = email, RoleId = customerRole.Id };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.Customers.Add(new Customer { UserId = user.Id, Phone = phone.Trim() });
        await _context.SaveChangesAsync();

        TempData["Message"] = "Registration successful. You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet] public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        { ModelState.AddModelError("", "Please enter your email and password."); return View(); }

        email = email.Trim().ToLower();

        var user = await _context.Users.Include(u => u.Role).Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
        { ModelState.AddModelError("", "Invalid email or password."); return View(); }

        if (user.Customer is null)
        { ModelState.AddModelError("", "This account is not linked to a customer profile."); return View(); }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new("CustomerId", user.Customer.Id.ToString()),
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ---------- Booking flow (unchanged business rules, now scoped to the signed-in customer) ----------

    public async Task<IActionResult> Dashboard()
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));
        var mine = (await _bookings.GetAllAsync()).Where(b => b.CustomerId == CurrentCustomerId);
        return View(mine);
    }

    public async Task<IActionResult> Browse(string? category)
    {
        ViewBag.Category = category;
        return View(await _catalog.GetServicesAsync(category));
    }

    [HttpGet]
    public async Task<IActionResult> Booking()
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));
        ViewBag.Units = await _catalog.GetServicesAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Booking(int unitId, DateTime date, string location, string? notes)
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(location) || date.Date < DateTime.Today)
        {
            ModelState.AddModelError("", "Please complete all required fields and choose a valid date.");
            ViewBag.Units = await _catalog.GetServicesAsync();
            return View();
        }

        var result = await _bookings.CreateAsync(new NewBookingRequest
        {
            CustomerId = CurrentCustomerId,
            ServiceItemId = unitId,
            Location = location,
            Notes = notes,
            StartDate = date,
            EndDate = date.AddDays(1), // TODO: add an End Date field to the form for multi-day rentals
        });

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            ViewBag.Units = await _catalog.GetServicesAsync();
            return View();
        }

        TempData["BookingReference"] = result.Value!.Reference;
        TempData["BookingService"] = result.Value.Service;
        return RedirectToAction(nameof(Confirmation));
    }

    [HttpGet]
    public IActionResult UploadProof()
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProof(string reference, IFormFile? proof)
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));

        if (string.IsNullOrWhiteSpace(reference) || proof is null || proof.Length == 0)
        { ModelState.AddModelError("", "Please enter a booking reference and choose a file."); return View(); }

        await using var stream = proof.OpenReadStream();
        var saveResult = await _fileStorage.SaveAsync(stream, proof.FileName, proof.ContentType, proof.Length);
        if (!saveResult.Success)
        { ModelState.AddModelError("", saveResult.Error!); return View(); }

        var submitResult = await _payments.SubmitProofAsync(reference, CurrentCustomerId, saveResult.Value!);
        if (!submitResult.Success)
        { ModelState.AddModelError("", submitResult.Error!); return View(); }

        TempData["Message"] = "Proof of payment submitted successfully";
        return RedirectToAction(nameof(MyBookings));
    }

    public IActionResult Confirmation() => View();

    public async Task<IActionResult> MyBookings()
    {
        if (!IsLoggedIn) return RedirectToAction(nameof(Login));
        var mine = (await _bookings.GetAllAsync()).Where(b => b.CustomerId == CurrentCustomerId);
        return View(mine);
    }
}
