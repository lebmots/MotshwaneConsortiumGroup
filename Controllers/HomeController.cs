using Microsoft.AspNetCore.Mvc;
namespace MotshwaneConsortiumGroup.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
