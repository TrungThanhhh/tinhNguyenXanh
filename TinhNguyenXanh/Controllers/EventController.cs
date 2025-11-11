using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;
        private readonly IEventRegistrationService _regService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(
            IEventService service,
            IEventRegistrationService regService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _regService = regService ?? throw new ArgumentNullException(nameof(regService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        // [1] DASHBOARD TÌNH NGUYỆN VIÊN
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var stats = new VolunteerDashboardDTO
            {
                TotalEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id),
                CompletedEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed"),
                PendingEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Pending"),
                TotalHours = await _context.EventRegistrations
                    .Where(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed")
                    .Join(_context.Events, reg => reg.EventId, evt => evt.Id, (reg, evt) =>
                        EF.Functions.DateDiffHour(evt.StartTime, evt.EndTime))
                    .SumAsync()
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
                var volunteer = await GetOrCreateVolunteerAsync(userId);
                var reg = await _context.EventRegistrations
                    .FirstOrDefaultAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
                ViewBag.RegistrationStatus = reg?.Status;
            }

            return View(evt);
        }

        // [4] FORM ĐĂNG KÝ (GET)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null || evt.Status != "approved") return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

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
        [Authorize]
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
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            // Cập nhật thông tin Volunteer từ form
            volunteer.FullName = dto.FullName;
            volunteer.Phone = dto.Phone;

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

            TempData["Message"] = "Đăng ký thành công! Chờ duyệt.";
            return RedirectToAction("Details", new { id = dto.EventId });
        }

        // [6] DANH SÁCH ĐĂNG KÝ CỦA TÔI
        [Authorize]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);
            var regs = await _regService.GetByVolunteerIdAsync(volunteer.Id);
            return View(regs);
        }

        // === HÀM HỖ TRỢ: TỰ ĐỘNG TẠO VOLUNTEER ===
        private async Task<Volunteer> GetOrCreateVolunteerAsync(string userId)
        {
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer != null) return volunteer;

            var user = await _userManager.FindByIdAsync(userId);
            volunteer = new Volunteer
            {
                UserId = userId,
                FullName = user?.FullName ?? user?.UserName ?? "Tình nguyện viên",
                Phone = user?.PhoneNumber,
                JoinedDate = DateTime.UtcNow
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();
            return volunteer;
        }
    }
}