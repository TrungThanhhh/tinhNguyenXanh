using Microsoft.AspNetCore.Mvc;

namespace TinhNguyenXanh.Areas.Volunteer.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
