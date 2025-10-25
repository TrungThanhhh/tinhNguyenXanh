using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;

        public EventController(IEventService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _service.GetAllEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound();

            ViewBag.Message = TempData["Message"];
            return View(evt);
        }

        [HttpPost]
        [Authorize] // Chỉ yêu cầu đăng nhập khi đăng ký
        public async Task<IActionResult> Register(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "Vui lòng đăng nhập để đăng ký.";
                return RedirectToAction("Details", new { id });
            }

            var success = await _service.RegisterForEventAsync(id, userId);
            if (success)
            {
                TempData["Message"] = "Đăng ký thành công!";
            }
            else
            {
                TempData["Message"] = "Đăng ký thất bại ";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}