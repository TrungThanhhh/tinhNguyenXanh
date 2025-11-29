// Areas/Admin/Controllers/EventsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách hoạt động
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 10;

            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Include(e => e.Reports)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(s) ||
                    (e.Organization != null && e.Organization.Name.ToLower().Contains(s)));
            }

            var total = await query.CountAsync();

            var events = await query
                .OrderByDescending(e => e.Id) // Dùng Id thay CreatedAt
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventAdminListViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    OrganizerName = e.Organization != null ? e.Organization.Name : "Không xác định",
                    CategoryName = e.Category != null ? e.Category.Name : "Chưa có",
                    StartTime = e.StartTime,
                    IsHidden = e.IsHidden,
                    ReportCount = e.Reports.Count,
                    ImageUrl = e.Images,
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(events);
        }

        // Chi tiết + danh sách báo cáo
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Include(e => e.Reports!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            var model = new EventDetailViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                ImageUrl = ev.Images != null ? ev.Images.Split(',')[0] : null,
                StartTime = ev.StartTime,
                Location = ev.Location,
                OrganizerName = ev.Organization?.Name ?? "Không xác định",
                CategoryName = ev.Category?.Name ?? "Chưa có",
                IsHidden = ev.IsHidden,
                HiddenReason = ev.HiddenReason,
                HiddenAt = ev.HiddenAt,
                Reports = ev.Reports.Select(r => new ReportViewModel
                {
                    ReporterName = r.User.FullName ?? r.User.Email,
                    ReporterEmail = r.User.Email,
                    Reason = r.ReportReason,
                    ReportedAt = r.ReportDate
                }).OrderByDescending(r => r.ReportedAt).ToList()
            };

            return View(model);
        }

        // Ẩn / Hiện (có nhập lý do)
        [HttpPost]
        public async Task<IActionResult> ToggleHide(int id, string? reason = null)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return Json(new { success = false, message = "Không tìm thấy!" });

            if (ev.IsHidden)
            {
                ev.IsHidden = false;
                ev.HiddenReason = null;
                ev.HiddenAt = null;
            }
            else
            {
                ev.IsHidden = true;
                ev.HiddenReason = string.IsNullOrWhiteSpace(reason) ? "Vi phạm nội dung" : reason.Trim();
                ev.HiddenAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = ev.IsHidden ? $"Đã ẩn ({ev.HiddenReason})" : "Đã hiện lại" });
        }

        // Xóa vĩnh viễn
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Reports)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return Json(new { success = false });

            _context.EventReports.RemoveRange(ev.Reports);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa hoạt động và toàn bộ báo cáo!" });
        }
    }
}