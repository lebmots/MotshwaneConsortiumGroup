using Microsoft.AspNetCore.Mvc;
using MotshwaneConsortiumGroup.Services;
namespace MotshwaneConsortiumGroup.Controllers;
public class StaffController : Controller
{
    private readonly DemoDataService _data;
    public StaffController(DemoDataService data) => _data = data;
    public IActionResult Dashboard() => View(_data.Jobs);
    public IActionResult Jobs() => View(_data.Jobs);
    public IActionResult JobDetails(int id=1) => View(_data.Jobs.FirstOrDefault(x => x.Id == id) ?? _data.Jobs.First());
    [HttpGet] public IActionResult UpdateStatus(int id=1) => View(_data.Jobs.FirstOrDefault(x => x.Id == id) ?? _data.Jobs.First());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(int id, string status, string? notes)
    {
        var job = _data.Jobs.FirstOrDefault(x => x.Id == id);
        if (job is not null && !string.IsNullOrWhiteSpace(status)) job.Status = status;
        TempData["Message"] = "Job status updated successfully";
        return RedirectToAction(nameof(Jobs));
    }
    public IActionResult Profile() => View();
}
