using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;

namespace MotshwaneConsortiumGroup.Controllers;

public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var jobs = await _context.StaffJobs
            .Include(j => j.Booking)
                .ThenInclude(b => b.Unit)
            .AsNoTracking()
            .ToListAsync();

        return View(jobs);
    }

    public async Task<IActionResult> Jobs()
    {
        var jobs = await _context.StaffJobs
            .Include(j => j.Booking)
                .ThenInclude(b => b.Unit)
            .AsNoTracking()
            .ToListAsync();

        return View(jobs);
    }

    public async Task<IActionResult> JobDetails(int id)
    {
        var job = await _context.StaffJobs
            .Include(j => j.Booking)
                .ThenInclude(b => b.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var job = await _context.StaffJobs
            .Include(j => j.Booking)
                .ThenInclude(b => b.Unit)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        string status,
        string? notes)
    {
        var job = await _context.StaffJobs
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(status))
        {
            job.Status = status;
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Job status updated successfully.";

        return RedirectToAction(nameof(Jobs));
    }

    public IActionResult Profile()
    {
        return View();
    }
}