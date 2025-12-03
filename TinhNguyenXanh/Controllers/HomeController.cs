using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.ViewModel;

namespace TinhNguyenXanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? keyword = "", int? category = null, string? location = "")
        {
            // Query sự kiện
            var eventQuery = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Where(e => e.Status == "approved");

            if (!string.IsNullOrWhiteSpace(keyword))
                eventQuery = eventQuery.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));

            if (category.HasValue && category.Value > 0)
                eventQuery = eventQuery.Where(e => e.CategoryId == category.Value);

            if (!string.IsNullOrWhiteSpace(location))
                eventQuery = eventQuery.Where(e => e.Location.Contains(location));

            var events = await eventQuery.ToListAsync();

            // Query tổ chức
            var orgQuery = _context.Organizations.Where(o => o.IsActive && o.Verified);

            if (!string.IsNullOrWhiteSpace(keyword))
                orgQuery = orgQuery.Where(o => o.Name.Contains(keyword) || o.Description.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(location))
                orgQuery = orgQuery.Where(o => o.Address.Contains(location));

            var organizations = await orgQuery.ToListAsync();

            var model = new SearchResultsViewModel
            {
                Keyword = keyword,
                CategoryId = category,
                Location = location,
                Events = events,
                Organizations = organizations
            };

            return View(model); // sẽ render ra Views/Home/Search.cshtml
        }






        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Donate()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
