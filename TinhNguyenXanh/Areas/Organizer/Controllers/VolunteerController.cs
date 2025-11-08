using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class VolunteersController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Certificates()
    {
        return View();
    }
}