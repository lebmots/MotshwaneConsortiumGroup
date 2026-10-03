using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Controllers;

public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
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
                "",
                "Please complete all fields.");

            return View();
        }

        if (password.Length < 8)
        {
            ModelState.AddModelError(
                "",
                "Password must be at least 8 characters.");

            return View();
        }

        email = email.Trim().ToLower();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "",
                "An account with this email already exists.");

            return View();
        }

        var customerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Customer");

        if (customerRole == null)
        {
            ModelState.AddModelError(
                "",
                "Customer role was not found in the database.");

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

        var customer = new Customer
        {
            UserId = user.Id,
            Phone = phone.Trim()
        };

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Registration successful. You can now sign in.";

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "",
                "Please enter your email and password.");

            return View();
        }

        email = email.Trim().ToLower();

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            return View();
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            return View();
        }

        if (user.Customer == null)
        {
            ModelState.AddModelError(
                "",
                "This account is not linked to a customer profile.");

            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role.Name),

            new Claim(
                "CustomerId",
                user.Customer.Id.ToString())
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(nameof(Login));
        }

        var customerId = int.Parse(
            User.FindFirstValue("CustomerId")!);

        var bookings = await _context.Bookings
            .Include(b => b.Unit)
            .Where(b => b.CustomerId == customerId)
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Browse(string? category)
    {
        var query = _context.Units
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(u =>
                u.Category == category);
        }

        var units = await query
            .OrderBy(u => u.Name)
            .ToListAsync();

        ViewBag.Category = category;

        return View(units);
    }

    [HttpGet]
    public async Task<IActionResult> Booking()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(nameof(Login));
        }

        var units = await _context.Units
            .Where(u => u.Available)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        ViewBag.Units = units;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Booking(
        int unitId,
        DateTime date,
        string location,
        string? notes)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(nameof(Login));
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            ModelState.AddModelError(
                "",
                "Location is required.");

            await LoadBookingUnits();
            return View();
        }

        if (date.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "",
                "Please choose a valid future date.");

            await LoadBookingUnits();
            return View();
        }

        var customerId = int.Parse(
            User.FindFirstValue("CustomerId")!);

        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == customerId);

        if (!customerExists)
        {
            ModelState.AddModelError(
                "",
                "Customer account could not be found.");

            await LoadBookingUnits();
            return View();
        }

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId);

        if (unit == null)
        {
            ModelState.AddModelError(
                "",
                "Selected unit does not exist.");

            await LoadBookingUnits();
            return View();
        }

        if (!unit.Available)
        {
            ModelState.AddModelError(
                "",
                "Selected unit is currently unavailable.");

            await LoadBookingUnits();
            return View();
        }

        var alreadyBooked = await _context.Bookings
            .AnyAsync(b =>
                b.UnitId == unitId &&
                b.BookingDate.Date == date.Date &&
                b.Status != "Cancelled");

        if (alreadyBooked)
        {
            ModelState.AddModelError(
                "",
                "This unit is already booked for that date.");

            await LoadBookingUnits();
            return View();
        }

        var booking = new Booking
        {
            Reference =
                "MC-" + Random.Shared.Next(100000, 999999),

            CustomerId = customerId,

            UnitId = unitId,

            BookingDate = date,

            Location = location.Trim(),

            Notes = notes,

            Status = "Pending",

            PaymentStatus = "Awaiting Proof"
        };

        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        TempData["BookingReference"] =
            booking.Reference;

        TempData["BookingService"] =
            unit.Category;

        return RedirectToAction(nameof(Confirmation));
    }

    private async Task LoadBookingUnits()
    {
        ViewBag.Units = await _context.Units
            .Where(u => u.Available)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    [HttpGet]
    public IActionResult UploadProof()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(nameof(Login));
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProof(
        string reference,
        IFormFile? proof)
    {
        if (string.IsNullOrWhiteSpace(reference) ||
            proof == null ||
            proof.Length == 0)
        {
            ModelState.AddModelError(
                "",
                "Please enter a booking reference and choose a file.");

            return View();
        }

        var customerId = int.Parse(
            User.FindFirstValue("CustomerId")!);

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b =>
                b.Reference == reference &&
                b.CustomerId == customerId);

        if (booking == null)
        {
            ModelState.AddModelError(
                "",
                "Booking could not be found.");

            return View();
        }

        booking.PaymentStatus = "Proof Submitted";

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Proof of payment submitted successfully.";

        return RedirectToAction(nameof(MyBookings));
    }

    public IActionResult Confirmation()
    {
        return View();
    }

    public async Task<IActionResult> MyBookings()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(nameof(Login));
        }

        var customerId = int.Parse(
            User.FindFirstValue("CustomerId")!);

        var bookings = await _context.Bookings
            .Include(b => b.Unit)
            .Where(b => b.CustomerId == customerId)
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return View(bookings);
    }
}