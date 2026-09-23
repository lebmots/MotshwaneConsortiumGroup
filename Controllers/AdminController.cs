using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class AdminController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    private readonly ICustomerService _customers;
    private readonly IStaffJobService _jobs;
    private readonly IStaffService _staff;

    public AdminController(IBookingService bookings, ICatalogService catalog, ICustomerService customers,
        IStaffJobService jobs, IStaffService staff)
    {
        _bookings = bookings;
        _catalog = catalog;
        _customers = customers;
        _jobs = jobs;
        _staff = staff;
    }

    public async Task<IActionResult> Dashboard() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Bookings() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Services() => View(await _catalog.GetServicesAsync());
    public async Task<IActionResult> Customers() => View(await _customers.GetAllAsync());
    public async Task<IActionResult> Payments() => View(await _bookings.GetAllAsync());

    [HttpGet]
    public async Task<IActionResult> AssignStaff()
    {
        // Only Confirmed bookings without a job yet can be assigned.
        var jobs = await _jobs.GetAllAsync();
        var assignable = (await _bookings.GetAllAsync())
            .Where(b => b.Status == MotshwaneConsortiumGroup.Models.BookingStatus.Confirmed
                        && !jobs.Any(j => j.BookingId == b.Id));
        ViewBag.Staff = await _staff.GetAllAsync();
        return View(assignable);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStaff(int bookingId, int staffId)
    {
        var result = await _jobs.AssignAsync(bookingId, staffId);
        TempData[result.Success ? "Message" : "Error"] = result.Success
            ? $"Assigned to job {result.Value!.Reference}."
            : result.Error;
        return RedirectToAction(nameof(AssignStaff));
    }

    public async Task<IActionResult> Reports() => View(await _bookings.GetAllAsync());
}
