using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MotshwaneConsortiumGroup.Services;
namespace MotshwaneConsortiumGroup.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly DemoDataService _data;
    public AdminController(DemoDataService data) => _data = data;
    public IActionResult Dashboard() => View(_data.Bookings);
    public IActionResult Bookings() => View(_data.Bookings);
    public IActionResult Services() => View(_data.Services);
    public IActionResult Customers() => View(_data.Customers);
    public IActionResult Payments() => View(_data.Bookings);
    public IActionResult AssignStaff() => View(_data.Bookings);
    public IActionResult Reports() => View(_data.Bookings);
}
