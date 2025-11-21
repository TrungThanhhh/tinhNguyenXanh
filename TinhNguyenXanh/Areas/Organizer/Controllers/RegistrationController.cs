using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Authorize(Roles = "Organizer")]
    public class RegistrationController : Controller
    {
        private readonly IEventRegistrationService _service;

        public RegistrationController(IEventRegistrationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IActionResult> Index(int? eventId)
        {
            IEnumerable<EventRegistration> regs;

            if (eventId.HasValue)
            {
                regs = await _service.GetByEventIdAsync(eventId.Value);
            }
            else
            {
                // Lấy tất cả đăng ký của tổ chức (nếu cần)
                // Hoặc để trống để hiển thị form chọn sự kiện
                regs = Enumerable.Empty<EventRegistration>();
            }

            return View(regs);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var success = await _service.ApproveAsync(id);
            if (!success)
            {
                TempData["Error"] = "Không thể duyệt đăng ký.";
            }
            else
            {
                TempData["Message"] = "Đã duyệt thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var success = await _service.RejectAsync(id);
            if (!success)
            {
                TempData["Error"] = "Không thể từ chối đăng ký.";
            }
            else
            {
                TempData["Message"] = "Đã từ chối đăng ký.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}