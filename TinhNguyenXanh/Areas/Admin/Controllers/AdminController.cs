using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

// Đặt controller này trong khu vực Admin
namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IEventRepository _eventRepo;
        private readonly IEventCategoryRepository _categoryRepo;
        private readonly IEventReportRepository _reportRepo;
        private readonly IOrganizationRepository _orgRepo;
        private readonly IUserRepository _userRepo;
        private readonly IStatisticRepository _statRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IEventRepository eventRepo,
            IEventCategoryRepository categoryRepo,
            IEventReportRepository reportRepo,
            IOrganizationRepository orgRepo,
            IUserRepository userRepo,
            IStatisticRepository statRepo,
            UserManager<ApplicationUser> userManager)
        {
            _eventRepo = eventRepo;
            _categoryRepo = categoryRepo;
            _reportRepo = reportRepo;
            _orgRepo = orgRepo;
            _userRepo = userRepo;
            _statRepo = statRepo;
            _userManager = userManager;
        }

        // ====================== DASHBOARD CHÍNH ======================
        [Area("Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalEvents = await _statRepo.GetTotalEventsAsync(),
                TotalVolunteers = await _statRepo.GetTotalVolunteersAsync(),
                Top5FavoriteEvents = await _statRepo.GetTopFavoriteEventsAsync(5),
                TotalReportsPending = await _reportRepo.GetAllReportsAsync()
                    .ContinueWith(t => t.Result.Count(r => r.Status == "Pending")),
                TotalOrganizations = await _orgRepo.GetAllAsync().ContinueWith(t => t.Result.Count()),
                TotalUsers = await _userRepo.GetAllUsersAsync().ContinueWith(t => t.Result.Count())
            };

            return View(model);
        }

        // ====================== 1. QUẢN LÝ TÀI KHOẢN ======================
        [Area("Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> ManageUsers(int page = 1, int pageSize = 15)
        {
            var users = await _userRepo.GetAllUsersAsync();

            var userViewModels = new List<UserAdminViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "Volunteer";

                userViewModels.Add(new UserAdminViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "Không tên",
                    Email = user.Email ?? "Chưa có email",
                    RegisteredDate = user.RegisteredDate, // Bây giờ đã có rồi!
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now,
                    Role = primaryRole
                });
            }

            var pagedUsers = userViewModels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(userViewModels.Count / (double)pageSize);

            return View(pagedUsers);
        }

        // ====================== 2. QUẢN LÝ HOẠT ĐỘNG ======================
        [Area("Admin")]
        [HttpGet("events")]
        public async Task<IActionResult> ManageEvents(string status = "all", int page = 1, int pageSize = 15)
        {
            var eventsQuery = await _eventRepo.GetAllEventsAsync();
            var events = eventsQuery.AsQueryable();

            if (status != "all")
                events = events.Where(e => e.Status == status);

            var total = events.Count();
            var items = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Events = items;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentStatus = status;

            return View("ManageEvents");
        }

        [HttpPost("events/hide/{id}")]
        public async Task<IActionResult> HideEvent(int id)
        {
            var evt = await _eventRepo.GetEventByIdAsync(id);
            if (evt != null)
            {
                evt.Status = "hidden"; // hoặc thêm trạng thái "banned"
                await _eventRepo.SaveChangesAsync();
                TempData["Message"] = "Đã ẩn hoạt động";
            }
            return RedirectToAction("ManageEvents");
        }

        [HttpPost("events/approve/{id}")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var evt = await _eventRepo.GetEventByIdAsync(id);
            if (evt != null)
            {
                evt.Status = "approved";
                await _eventRepo.SaveChangesAsync();
                TempData["Message"] = "Đã duyệt hoạt động";
            }
            return RedirectToAction("ManageEvents");
        }

        // ====================== 3. QUẢN LÝ DANH MỤC ======================
        [Area("Admin")]
        [HttpGet("categories")]
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            return View("ManageCategories", categories);
        }

        [HttpPost("categories/add")]
        public async Task<IActionResult> AddCategory(EventCategory category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepo.AddCategoryAsync(category);
                TempData["Message"] = "Thêm danh mục thành công";
            }
            return RedirectToAction("ManageCategories");
        }

        [HttpPost("categories/update")]
        public async Task<IActionResult> UpdateCategory(EventCategory category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepo.UpdateCategoryAsync(category);
                TempData["Message"] = "Cập nhật thành công";
            }
            return RedirectToAction("ManageCategories");
        }

        [HttpPost("categories/delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepo.DeleteCategoryAsync(id);
            TempData["Message"] = "Đã xóa danh mục";
            return RedirectToAction("ManageCategories");
        }

        // ====================== 4. QUẢN LÝ BÁO CÁO VI PHẠM ======================
        [Area("Admin")]
        [HttpGet("reports")]
        public async Task<IActionResult> ManageReports(string status = "all")
        {
            var reports = await _reportRepo.GetAllReportsAsync();
            if (status != "all")
                reports = reports.Where(r => r.Status == status);

            ViewBag.Reports = reports.OrderByDescending(r => r.ReportDate);
            ViewBag.CurrentStatus = status;
            return View("ManageReports");
        }

        [HttpPost("reports/update-status/{reportId}")]
        public async Task<IActionResult> UpdateReportStatus(int reportId, string newStatus)
        {
            var success = await _reportRepo.UpdateReportStatusAsync(reportId, newStatus);
            TempData["Message"] = success ? "Cập nhật trạng thái báo cáo thành công" : "Không tìm thấy báo cáo";
            return RedirectToAction("ManageReports");
        }

        [HttpPost("reports/delete/{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            await _reportRepo.DeleteReportAsync(reportId);
            TempData["Message"] = "Đã xóa báo cáo";
            return RedirectToAction("ManageReports");
        }

        // ====================== 5. THỐNG KÊ ======================
        [Area("Admin")]
        [HttpGet("statistics")]
        public async Task<IActionResult> Statistics()
        {
            var model = new AdminStatisticsViewModel
            {
                TotalEvents = await _statRepo.GetTotalEventsAsync(),
                TotalVolunteers = await _statRepo.GetTotalVolunteersAsync(),
                Top5FavoriteEvents = await _statRepo.GetTopFavoriteEventsAsync(5),
                PendingReports = await _reportRepo.GetAllReportsAsync()
                    .ContinueWith(t => t.Result.Count(r => r.Status == "Pending"))
            };

            return View("Statistics", model);
        }
    }
}