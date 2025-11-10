using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // THÊM DÒNG NÀY
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Event.Controllers // ĐÚNG NAMESPACE
{
    [Area("Event")]
    public class EventController : Controller
    {
        private readonly IEventService _service;
        private readonly ApplicationDbContext _context;

        public EventController(IEventService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var events = await _service.GetApprovedEventsAsync();
            return View(events);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null) return NotFound();

            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var volunteer = await _context.Volunteers
                    .FirstOrDefaultAsync(v => v.UserId == userId);

                ViewBag.IsVolunteer = volunteer != null;

                if (volunteer != null)
                {
                    var reg = await _context.EventRegistrations
                        .FirstOrDefaultAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
                    ViewBag.RegistrationStatus = reg?.Status;
                }
            }

            return View(evt);
        }

        [HttpGet]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null || evt.Status != "approved") return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null)
            {
                TempData["Error"] = "Bạn cần hoàn thiện hồ sơ tình nguyện viên.";
                return RedirectToAction("CompleteProfile", "Profile", new { area = "Volunteer" });
            }

            var dto = new EventRegistrationDTO
            {
                EventId = evt.Id,
                EventTitle = evt.Title,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Location = evt.Location,
                FullName = volunteer.FullName ?? "",
                Phone = volunteer.Phone ?? ""
            };

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Volunteer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEvent(EventRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var evt = await _service.GetEventByIdAsync(dto.EventId);
                if (evt != null)
                {
                    dto.EventTitle = evt.Title;
                    dto.StartTime = evt.StartTime;
                    dto.EndTime = evt.EndTime;
                    dto.Location = evt.Location;
                }
                return View(dto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ tình nguyện viên.";
                return RedirectToAction("Details", new { area = "", id = dto.EventId });
            }

            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == dto.EventId && r.VolunteerId == volunteer.Id);

            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này rồi.";
                return RedirectToAction("Details", new { area = "", id = dto.EventId });
            }

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                VolunteerId = volunteer.Id,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Reason = dto.Reason,
                Status = "Pending",
                RegisteredDate = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Gửi đăng ký thành công! Ban tổ chức sẽ sớm phản hồi.";
            return RedirectToAction("Details", new { area = "", id = dto.EventId });
        }
    }
}