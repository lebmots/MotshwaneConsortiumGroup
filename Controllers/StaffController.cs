using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services.Interfaces;
namespace MotshwaneConsortiumGroup.Controllers;
public class StaffController : Controller
{
    private readonly IStaffJobService _jobs;
    public StaffController(IStaffJobService jobs) => _jobs = jobs;

    public async Task<IActionResult> Dashboard() => View(await _jobs.GetAllAsync());
    public async Task<IActionResult> Jobs() => View(await _jobs.GetAllAsync());

    public async Task<IActionResult> JobDetails(int id = 1) =>
        View(await _jobs.GetByIdAsync(id) ?? (await _jobs.GetAllAsync()).First());

    [HttpGet]
    public async Task<IActionResult> UpdateStatus(int id = 1) =>
        View(await _jobs.GetByIdAsync(id) ?? (await _jobs.GetAllAsync()).First());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? notes)
    {
        var result = await _jobs.UpdateStatusAsync(id, status);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(UpdateStatus), new { id });
        }
        TempData["Message"] = "Job status updated successfully";
        return RedirectToAction(nameof(Jobs));
    }

    public IActionResult Profile() => View();
}
