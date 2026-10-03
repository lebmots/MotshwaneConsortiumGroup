using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;

namespace MotshwaneConsortiumGroup.Controllers;

public class AdminController : Controller
{
    private readonly DemoDataService _data;
    public AdminController(DemoDataService data) => _data = data;
    public IActionResult Dashboard() => View(_data.Bookings);
    public IActionResult Bookings() => View(_data.Bookings);
    public IActionResult Services() => View(_data.Units);
    public IActionResult Customers() => View(_data.Customers);
    public IActionResult Payments() => View(_data.Bookings);
    public IActionResult AssignStaff() => View(_data.Bookings);
    public IActionResult Reports() => View(_data.Bookings);
}
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var bookings = await GetBookings();

        return View(bookings);
    }

    public async Task<IActionResult> Bookings()
    {
        var bookings = await _context.Bookings
        .Include(b => b.Customer)
            .ThenInclude(c => c.User)
        .Include(b => b.Unit)
        .AsNoTracking()
        .OrderByDescending(b => b.BookingDate)
        .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Services()
    {
        var units = await _context.Units
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return View(units);
    }

    public async Task<IActionResult> Customers()
    {
        var customers = await _context.Customers
            .Include(c => c.User)
            .AsNoTracking()
            .OrderBy(c => c.User.FullName)
            .ToListAsync();

        return View(customers);
    }

    public async Task<IActionResult> Payments()
    {
        var bookings = await GetBookings();

        return View(bookings);
    }

    public async Task<IActionResult> AssignStaff()
    {
        var bookings = await GetBookings();

        return View(bookings);
    }

    public async Task<IActionResult> Reports()
    {
        var bookings = await GetBookings();

        return View(bookings);
    }

    private async Task<List<MotshwaneConsortiumGroup.Models.Booking>>
        GetBookings()
    {
        return await _context.Bookings
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .Include(b => b.Unit)
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
}