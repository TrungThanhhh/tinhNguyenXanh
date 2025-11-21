// Areas/Organizer/Controllers/VolunteersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Authorize(Roles = "Organizer")]
    public class VolunteersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [1] DANH SÁCH ĐĂNG KÝ – GIỐNG HỆT ẢNH
        public async Task<IActionResult> Index(int? eventId, string search, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organization == null)
                return NotFound("Không tìm thấy tổ chức của bạn.");

            // Lấy danh sách sự kiện của BTC
            var events = await _context.Events
                .Where(e => e.OrganizationId == organization.Id)
                .Select(e => new { e.Id, e.Title })
                .ToListAsync();

            ViewBag.EventList = new SelectList(events, "Id", "Title", eventId);
            ViewBag.Search = search;

            // Query đăng ký chờ duyệt
            var query = _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Include(r => r.Event)
                .Where(r => r.Event.OrganizationId == organization.Id && r.Status == "Pending");

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.FullName.Contains(search) || r.Phone.Contains(search));

            // Phân trang: 9 người/trang
            int pageSize = 9;
            var total = await query.CountAsync();
            var registrations = await query
                .OrderByDescending(r => r.RegisteredDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(registrations);
        }

        // [2] DUYỆT ĐĂNG KÝ
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var reg = await _context.EventRegistrations.FindAsync(id);
            if (reg == null || reg.Status != "Pending")
            {
                TempData["Error"] = "Không thể duyệt.";
                return RedirectToAction(nameof(Index));
            }

            reg.Status = "Confirmed";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã duyệt {reg.FullName}";
            return RedirectToAction(nameof(Index));
        }

        // [3] TỪ CHỐI ĐĂNG KÝ
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var reg = await _context.EventRegistrations.FindAsync(id);
            if (reg == null || reg.Status != "Pending")
            {
                TempData["Error"] = "Không thể từ chối.";
                return RedirectToAction(nameof(Index));
            }

            reg.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Error"] = $"Đã từ chối {reg.FullName}";
            return RedirectToAction(nameof(Index));
        }

        // [4] XEM GIẤY XÁC NHẬN (GIỮ LẠI)
        public IActionResult Certificates()
        {
            return View();
        }
    }
}