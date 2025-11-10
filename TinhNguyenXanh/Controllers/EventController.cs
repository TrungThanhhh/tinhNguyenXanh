// Controllers/EventController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Controllers
{
    [Authorize(Roles = "Volunteer")]
    public class EventController : Controller
    {
        private readonly IEventService _service;
        private readonly IEventRegistrationService _regService;
        private readonly ApplicationDbContext _context;

        public EventController(IEventService service, IEventRegistrationService regService, ApplicationDbContext context)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _regService = regService ?? throw new ArgumentNullException(nameof(regService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // [1] DASHBOARD TÌNH NGUYỆN VIÊN
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null)
                return RedirectToAction("CompleteProfile");

            var stats = new VolunteerDashboardDTO
            {
                TotalEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id),
                CompletedEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed"),
                PendingEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Pending"),
                TotalHours = await _context.EventRegistrations
                    .Where(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed")
                    .SumAsync(r => EF.Functions.DateDiffHour(r.Event.StartTime, r.Event.EndTime))
            };

            return View(stats);
        }

        // [2] DANH SÁCH SỰ KIỆN CÔNG KHAI
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var events = await _service.GetApprovedEventsAsync();
            return View(events);
        }

        // [3] CHI TIẾT SỰ KIỆN
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
                var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
                ViewBag.IsVolunteer = volunteer != null;
            }

            return View(evt);
        }

        // [4] FORM ĐĂNG KÝ (GET)
        [HttpGet]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null || evt.Status != "approved")
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer == null)
            {
                TempData["Error"] = "Vui lòng hoàn thiện hồ sơ.";
                return RedirectToAction("CompleteProfile");
            }

            // Kiểm tra đã đăng ký chưa
            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này.";
                return RedirectToAction("Details", new { id });
            }

            // Kiểm tra số lượng
            var currentCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == id && (r.Status == "Pending" || r.Status == "Confirmed"));
            if (currentCount >= evt.MaxVolunteers)
            {
                TempData["Error"] = "Sự kiện đã đủ số lượng tình nguyện viên.";
                return RedirectToAction("Details", new { id });
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

        // [5] GỬI ĐĂNG KÝ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEvent(EventRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var revt = await _service.GetEventByIdAsync(dto.EventId);
                if (revt != null)
                {
                    dto.EventTitle = revt.Title;
                    dto.StartTime = revt.StartTime;
                    dto.EndTime = revt.EndTime;
                    dto.Location = revt.Location;
                }
                return View(dto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer == null)
                return RedirectToAction("CompleteProfile");

            // Kiểm tra lại (race condition)
            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == dto.EventId && r.VolunteerId == volunteer.Id);
            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này.";
                return RedirectToAction("Details", new { id = dto.EventId });
            }

            var evt = await _context.Events.FindAsync(dto.EventId);
            if (evt == null || evt.Status != "approved")
            {
                TempData["Error"] = "Sự kiện không hợp lệ.";
                return RedirectToAction("Details", new { id = dto.EventId });
            }

            var currentCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == dto.EventId && (r.Status == "Pending" || r.Status == "Confirmed"));
            if (currentCount >= evt.MaxVolunteers)
            {
                TempData["Error"] = "Sự kiện đã đủ số lượng tình nguyện viên.";
                return RedirectToAction("Details", new { id = dto.EventId });
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

            TempData["Message"] = "Đăng ký thành công!";
            return RedirectToAction("Details", new { id = dto.EventId });
        }

        // [6] DANH SÁCH ĐĂNG KÝ CỦA TÔI
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer == null)
                return RedirectToAction("CompleteProfile");

            var regs = await _regService.GetByVolunteerIdAsync(volunteer.Id);
            return View(regs);
        }

        // [7] HOÀN THIỆN HỒ SƠ
        [HttpGet]
        public IActionResult CompleteProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = _context.Volunteers.Any(v => v.UserId == userId);
            if (existing)
                return RedirectToAction("Dashboard");

            return View(new Volunteer { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(Volunteer model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exists = await _context.Volunteers.AnyAsync(v => v.UserId == userId);
            if (exists)
            {
                TempData["Error"] = "Hồ sơ đã tồn tại.";
                return View(model);
            }

            model.JoinedDate = DateTime.UtcNow;
            _context.Volunteers.Add(model);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Hoàn thiện hồ sơ thành công!";
            return RedirectToAction("Dashboard");
        }
    }
}