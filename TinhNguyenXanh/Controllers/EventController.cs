using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;

        public EventController(IEventService service)
        {
            _service = service;
        }

        // Danh sách hoạt động đã được duyệt
        public async Task<IActionResult> Index()
        {
            var events = await _service.GetApprovedEventsAsync();
            return View(events);
        }

        // Chi tiết hoạt động
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound();

            ViewBag.Message = TempData["Message"];
            return View(evt);
        }

        // Đăng ký hoạt động
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "Vui lòng đăng nhập để đăng ký.";
                return RedirectToAction("Details", new { id });
            }

            var success = await _service.RegisterForEventAsync(id, userId);
            TempData["Message"] = success
                ? "Đăng ký thành công!"
                : "Đăng ký thất bại (hoạt động đã đầy hoặc chưa được duyệt).";

            return RedirectToAction("Details", new { id });
        }
    }
}
