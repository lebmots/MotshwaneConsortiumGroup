using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;

namespace MotshwaneConsortiumGroup.Controllers;

public class AdminController : Controller
{
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