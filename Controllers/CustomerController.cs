using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public CustomerController(
    IBookingService bookings,
    ICatalogService catalog,
    IPaymentService payments,
    IFileStorageService fileStorage,
    ApplicationDbContext context)
    {
        _bookings = bookings;
        _catalog = catalog;
        _payments = payments;
        _fileStorage = fileStorage;
        _context = context;
    }

    private int CurrentCustomerId =>
    int.Parse(User.FindFirstValue("CustomerId")!);

    // ---------------- REGISTRATION ----------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
    string name,
    string phone,
    string email,
    string password)
    {
        if (string.IsNullOrWhiteSpace(name) ||
        string.IsNullOrWhiteSpace(phone) ||
        string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
            "", "Please complete all fields.");
            return View();
        }

        if (password.Length < 8)
        {
            ModelState.AddModelError(
            "", "Password must be at least 8 characters.");
            return View();
        }

        email = email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(
        u => u.Email == email))
        {
            ModelState.AddModelError(
            "", "An account with this email already exists.");
            return View();
        }

        var customerRole = await _context.Roles
        .FirstOrDefaultAsync(r => r.Name == "Customer");

        if (customerRole is null)
        {
            ModelState.AddModelError(
            "", "Customer role was not found in the database.");
            return View();
        }

        var user = new User
        {
            FullName = name.Trim(),
            Email = email,
            RoleId = customerRole.Id
        };

        user.PasswordHash =
        _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.Customers.Add(new Customer
        {
            UserId = user.Id,
            Phone = phone.Trim()
        });

        await _context.SaveChangesAsync();

        TempData["Message"] =
        "Registration successful. You can now sign in.";

        return RedirectToAction(nameof(Login));
    }

    // ---------------- LOGIN ----------------

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
    string email,
    string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
            "", "Please enter your email and password.");
            return View();
        }

        email = email.Trim().ToLowerInvariant();

        var user = await _context.Users
        .Include(u => u.Role)
        .Include(u => u.Customer)
        .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null ||
        _passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(
            "", "Invalid email or password.");
            return View();
        }

        if (user.Customer is null ||
        user.Role.Name != "Customer")
        {
            ModelState.AddModelError(
            "", "This account is not linked to a customer profile.");
            return View();
        }

        var claims = new List<Claim>
{
new(ClaimTypes.NameIdentifier, user.Id.ToString()),
new(ClaimTypes.Name, user.FullName),
new(ClaimTypes.Email, user.Email),
new(ClaimTypes.Role, user.Role.Name),
new("CustomerId", user.Customer.Id.ToString())
};

        var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal);

        return RedirectToAction(nameof(Dashboard));
    }

    // ---------------- LOGOUT ----------------

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    // ---------------- CUSTOMER DASHBOARD ----------------

    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Dashboard()
    {
        var bookings = await _bookings.GetAllAsync();

        var myBookings = bookings.Where(
        b => b.CustomerId == CurrentCustomerId);

        return View(myBookings);
    }

    // ---------------- BROWSE SERVICES ----------------

    [AllowAnonymous]
    public async Task<IActionResult> Browse(string? category)
    {
        ViewBag.Category = category;

        var services =
        await _catalog.GetServicesAsync(category);

        return View(services);
    }

    // ---------------- CREATE BOOKING ----------------

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Booking()
    {
        ViewBag.Units = await _catalog.GetServicesAsync();

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Booking(
    int unitId,
    DateTime date,
    string location,
    string? notes)
    {
        if (unitId <= 0 ||
        string.IsNullOrWhiteSpace(location) ||
        date.Date < DateTime.Today)
        {
            ModelState.AddModelError(
            "",
            "Please complete all required fields and choose a valid date.");

            ViewBag.Units = await _catalog.GetServicesAsync();
            return View();
        }

        var result = await _bookings.CreateAsync(
        new NewBookingRequest
        {
            CustomerId = CurrentCustomerId,
            ServiceItemId = unitId,
            Location = location.Trim(),
            Notes = notes,
            StartDate = date,
            EndDate = date.AddDays(1)
        });

        if (!result.Success)
        {
            ModelState.AddModelError(
            "", result.Error ?? "Unable to create booking.");

            ViewBag.Units = await _catalog.GetServicesAsync();
            return View();
        }

        TempData["BookingReference"] =
        result.Value!.Reference;

        TempData["BookingService"] =
        result.Value.Service;

        return RedirectToAction(nameof(Confirmation));
    }

    // ---------------- UPLOAD PAYMENT PROOF ----------------

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public IActionResult UploadProof()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProof(
    string reference,
    IFormFile? proof)
    {
        if (string.IsNullOrWhiteSpace(reference) ||
        proof is null ||
        proof.Length == 0)
        {
            ModelState.AddModelError(
            "",
            "Please enter a booking reference and choose a file.");

            return View();
        }

        await using var stream = proof.OpenReadStream();

        var saveResult = await _fileStorage.SaveAsync(
        stream,
        proof.FileName,
        proof.ContentType,
        proof.Length);

        if (!saveResult.Success)
        {
            ModelState.AddModelError(
            "", saveResult.Error ?? "File upload failed.");

            return View();
        }

        var submitResult =
        await _payments.SubmitProofAsync(
        reference,
        CurrentCustomerId,
        saveResult.Value!);

        if (!submitResult.Success)
        {
            ModelState.AddModelError(
            "",
            submitResult.Error ?? "Unable to submit payment proof.");

            return View();
        }

        TempData["Message"] =
        "Proof of payment submitted successfully.";

        return RedirectToAction(nameof(MyBookings));
    }

    // ---------------- BOOKING CONFIRMATION ----------------

    [Authorize(Roles = "Customer")]
    public IActionResult Confirmation()
    {
        return View();
    }

    // ---------------- MY BOOKINGS ----------------

    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyBookings()
    {
        var bookings = await _bookings.GetAllAsync();

        var myBookings = bookings.Where(
        b => b.CustomerId == CurrentCustomerId);

        return View(myBookings);
    }
}

