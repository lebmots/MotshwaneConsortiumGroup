using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class AdminController : Controller
{
    private readonly IBookingService _bookings;
    private readonly ICatalogService _catalog;
    private readonly ICustomerService _customers;

    public AdminController(IBookingService bookings, ICatalogService catalog, ICustomerService customers)
    {
        _bookings = bookings;
        _catalog = catalog;
        _customers = customers;
    }

    public async Task<IActionResult> Dashboard() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Bookings() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Services() => View(await _catalog.GetServicesAsync());
    public async Task<IActionResult> Customers() => View(await _customers.GetAllAsync());
    public async Task<IActionResult> Payments() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> AssignStaff() => View(await _bookings.GetAllAsync());
    public async Task<IActionResult> Reports() => View(await _bookings.GetAllAsync());
}
