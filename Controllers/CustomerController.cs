using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class CustomerController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    public CustomerController(IBookingService bookings, ICatalogService catalog)
    {
        _bookings = bookings;
        _catalog = catalog;
    }
    public async Task<IActionResult> Dashboard() => View(await _bookings.GetAllAsync());
    [HttpGet] public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Register(string name, string phone, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
        { ModelState.AddModelError("", "Please complete all fields. Password must be at least 8 characters."); return View(); }
        TempData["Message"] = "Registration successful. You can sign in";
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public IActionResult Login() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        { ModelState.AddModelError("", "Please enter your email and password."); return View(); }
        return RedirectToAction(nameof(Dashboard));
    }
    public async Task<IActionResult> Browse(string? category)
    {
        ViewBag.Category = category;
        return View(await _catalog.GetServicesAsync(category));
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
    public async Task<IActionResult> MyBookings() => View(await _bookings.GetAllAsync());
}
