using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class CustomerController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    private readonly IPaymentService _payments;
    private readonly IFileStorageService _fileStorage;
    public CustomerController(IBookingService bookings, ICatalogService catalog, IPaymentService payments, IFileStorageService fileStorage)
    {
        _bookings = bookings;
        _catalog = catalog;
        _payments = payments;
        _fileStorage = fileStorage;
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
    public async Task<IActionResult> Booking(string name, string service, DateTime date, string location, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(location) || date.Date < DateTime.Today)
        { ModelState.AddModelError("", "Please complete all required fields and choose a valid date."); return View(); }

        // The form only collects a service NAME and one date today (no end date or unit id yet).
        // TODO(Lebone/Keren): add an End Date field and post ServiceItemId once it's on the form;
        // until then this assumes a 1-day rental and looks the item up by name.
        var match = (await _catalog.GetServicesAsync()).FirstOrDefault(
            s => s.Name.Equals(service, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        { ModelState.AddModelError("", "Selected service does not exist."); return View(); }

        var result = await _bookings.CreateAsync(new NewBookingRequest
        {
            CustomerName = name,
            ServiceItemId = match.Id,
            Location = location,
            StartDate = date,
            EndDate = date.AddDays(1),
        });

        if (!result.Success)
        { ModelState.AddModelError("", result.Error!); return View(); }

        TempData["BookingReference"] = result.Value!.Reference;
        TempData["BookingService"] = service;
        return RedirectToAction(nameof(Confirmation));
    }
    [HttpGet] public IActionResult UploadProof() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProof(string reference, IFormFile? proof)
    {
        if (string.IsNullOrWhiteSpace(reference) || proof is null || proof.Length == 0)
        { ModelState.AddModelError("", "Please enter a booking reference and choose a file."); return View(); }

        await using var stream = proof.OpenReadStream();
        var saveResult = await _fileStorage.SaveAsync(stream, proof.FileName, proof.ContentType, proof.Length);
        if (!saveResult.Success)
        { ModelState.AddModelError("", saveResult.Error!); return View(); }

        var submitResult = await _payments.SubmitProofAsync(reference, saveResult.Value!);
        if (!submitResult.Success)
        { ModelState.AddModelError("", submitResult.Error!); return View(); }

        TempData["Message"] = "Proof of payment submitted successfully";
        return RedirectToAction(nameof(MyBookings));
    }
    public IActionResult Confirmation() => View();
    public async Task<IActionResult> MyBookings() => View(await _bookings.GetAllAsync());
}
