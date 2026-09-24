using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class AdminController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    private readonly ICustomerService _customers;
    private readonly IStaffJobService _jobs;
    private readonly IStaffService _staff;
    private readonly IPaymentService _payments;

    public AdminController(IBookingService bookings, ICatalogService catalog, ICustomerService customers,
        IStaffJobService jobs, IStaffService staff, IPaymentService payments2)
    {
        _bookings = bookings;
        _catalog = catalog;
        _customers = customers;
        _jobs = jobs;
        _staff = staff;
        _payments = payments2;
    }

    public async Task<IActionResult> Dashboard() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Bookings() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Services() => View(await _catalog.GetServicesAsync());
    public async Task<IActionResult> Customers() => View(await _customers.GetAllAsync());

    public async Task<IActionResult> Payments() => View(await _bookings.GetAllAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePayment(int bookingId)
    {
        var payment = await _payments.GetByBookingIdAsync(bookingId);
        var result = payment is null
            ? OperationResult<Payment>.Fail("No proof has been submitted for this booking yet.")
            : await _payments.ApproveAsync(payment.Id);
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Payment approved. Booking confirmed." : result.Error;
        return RedirectToAction(nameof(Payments));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectPayment(int bookingId, string reason)
    {
        var payment = await _payments.GetByBookingIdAsync(bookingId);
        var result = payment is null
            ? OperationResult<Payment>.Fail("No proof has been submitted for this booking yet.")
            : await _payments.RejectAsync(payment.Id, reason);
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Payment rejected." : result.Error;
        return RedirectToAction(nameof(Payments));
    }

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
