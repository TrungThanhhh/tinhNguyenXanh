using Microsoft.AspNetCore.Mvc;

namespace TinhNguyenXanh.Areas.Organizer.Controllers
{
    public class OrganizationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
