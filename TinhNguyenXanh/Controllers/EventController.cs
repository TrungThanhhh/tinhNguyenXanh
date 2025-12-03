using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.ViewModel;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;
        private readonly IEventRegistrationService _regService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EventController(
            IEventService service,
            IEventRegistrationService regService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _regService = regService ?? throw new ArgumentNullException(nameof(regService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // [1] Volunteer dashboard
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

        // [2] Public events list (supports layout search redirect)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword = "", int? category = null, string? location = "")
        {
            var eventsQuery = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Where(e => e.Status == "approved");

            if (!string.IsNullOrWhiteSpace(keyword))
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));

            if (category.HasValue && category.Value > 0)
                eventsQuery = eventsQuery.Where(e => e.CategoryId == category.Value);

            if (!string.IsNullOrWhiteSpace(location))
                eventsQuery = eventsQuery.Where(e => e.Location.Contains(location));

            var events = await eventsQuery.ToListAsync();

            // also load organizations when redirected from Home search
            var orgQuery = _context.Organizations.Where(o => o.IsActive && o.Verified);
            if (!string.IsNullOrWhiteSpace(keyword))
                orgQuery = orgQuery.Where(o => o.Name.Contains(keyword) || o.Description.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(location))
                orgQuery = orgQuery.Where(o => o.Address.Contains(location));
            var organizations = await orgQuery.ToListAsync();

            // If your Event/Index view expects IEnumerable<EventDTO>, map here; otherwise pass entities
            // For now, return entities to existing view expecting DTOs only if service already maps.
            // Fallback to service for normal listing when no filters provided
            if (string.IsNullOrWhiteSpace(keyword) && !category.HasValue && string.IsNullOrWhiteSpace(location))
            {
                var approvedDtos = await _service.GetApprovedEventsAsync();
                return View(approvedDtos);
            }

            // When searching, show a dedicated SearchResultsViewModel-like view (reuse Event/Index if adapted)
            var model = new TinhNguyenXanh.Models.ViewModel.SearchResultsViewModel
            {
                Keyword = keyword,
                CategoryId = category,
                Location = location,
                Events = events,
                Organizations = organizations
            };
            return View("Search", model); // ensure you have Views/Event/Search.cshtml styled “wow”
        }

        // [3] Event details
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

        // [4] Register (GET)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null || evt.Status != "approved") return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này.";
                return RedirectToAction("Details", new { id });
            }

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

        // [5] Register (POST)
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

            volunteer.FullName = dto.FullName;
            volunteer.Phone = dto.Phone;

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

        // [6] My registrations
        [Authorize]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);
            var regs = await _regService.GetByVolunteerIdAsync(volunteer.Id);
            return View(regs);
        }

        // [7] Volunteer profile (GET)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var dto = new VolunteerProfileDTO
            {
                FullName = volunteer.FullName,
                Phone = volunteer.Phone,
                Email = volunteer.Email ?? User?.Identity?.Name,
                Address = volunteer.Address,
                Skills = volunteer.Skills,
                Bio = volunteer.Bio,
                AvatarUrl = volunteer.AvatarUrl
            };

            return View(dto);
        }

        // [8] Volunteer profile (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(VolunteerProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            volunteer.FullName = dto.FullName;
            volunteer.Phone = dto.Phone;
            volunteer.Email = dto.Email;
            volunteer.Address = dto.Address;
            volunteer.Skills = dto.Skills;
            volunteer.Bio = dto.Bio;

            // Keep avatar as is unless changed via UploadAvatar
            _context.Volunteers.Update(volunteer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        // [9] Upload avatar (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile? avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh hợp lệ.";
                return RedirectToAction("Profile");
            }

            // Basic validation: size < 5MB, image types only
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Định dạng ảnh không hỗ trợ. Vui lòng chọn JPG/PNG/WebP.";
                return RedirectToAction("Profile");
            }
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Ảnh quá lớn (>5MB). Vui lòng chọn ảnh nhỏ hơn.";
                return RedirectToAction("Profile");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            // Ensure folder exists: wwwroot/images/avatars
            var folder = Path.Combine(_env.WebRootPath, "images", "avatars");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            volunteer.AvatarUrl = $"/images/avatars/{fileName}";
            _context.Volunteers.Update(volunteer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật ảnh đại diện thành công!";
            return RedirectToAction("Profile");
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int eventId)
        {
            var comments = await _context.EventComments
                .Include(c => c.User)
                .Where(c => c.EventId == eventId && c.IsVisible && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return PartialView("_EventComments", comments);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment([FromForm] EventCommentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 1000)
                return BadRequest("Nội dung không hợp lệ.");

            var evt = await _context.Events.FindAsync(dto.EventId);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new EventComment
            {
                EventId = dto.EventId,
                UserId = userId,
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsVisible = true,
                IsDeleted = false
            };

            _context.EventComments.Add(comment);
            await _context.SaveChangesAsync();

            var created = await _context.EventComments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == comment.Id);
            return PartialView("_SingleComment", created);
        }


        // [C] Xóa hoặc ẩn comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.EventComments.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Cho phép xóa nếu là admin hoặc chủ tổ chức của event hoặc tác giả comment
            var isAdmin = roles.Contains("Admin");
            var isOrganizer = roles.Contains("Organizer") && comment.Event != null && comment.Event.OrganizationId == /* tổ chức id của user nếu có */ 0;
            var isAuthor = comment.UserId == userId;

            if (!isAdmin && !isAuthor && !isOrganizer)
            {
                return Forbid();
            }

            comment.IsDeleted = true;
            comment.IsVisible = false;
            _context.EventComments.Update(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // === Helper: ensure volunteer record exists ===
        private async Task<Volunteer> GetOrCreateVolunteerAsync(string userId)
        {
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer != null) return volunteer;

            var user = await _userManager.FindByIdAsync(userId);
            volunteer = new Volunteer
            {
                UserId = userId,
                FullName = user?.FullName ?? user?.UserName ?? "Tình nguyện viên",
                Email = user?.Email,
                Phone = user?.PhoneNumber,
                JoinedDate = DateTime.UtcNow,
                Availability = "Available"
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();
            return volunteer;
        }

    }
}
