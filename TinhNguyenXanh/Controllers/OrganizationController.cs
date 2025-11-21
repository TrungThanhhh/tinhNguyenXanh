using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrganizationController(
            IOrganizationService organizationService,
            UserManager<ApplicationUser> userManager)
        {
            _organizationService = organizationService;
            _userManager = userManager;
        }

        // GET: /Organization
        public async Task<IActionResult> Index()
        {
            var organizations = await _organizationService.GetAllAsync();
            return View(organizations);
        }

        // GET: /Organization/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var organization = await _organizationService.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        // GET: /Organization/Register
        [Authorize]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Organization/Register
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(OrganizationDTO model)
        {
            // Debug ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"[ModelState Invalid] Errors: {string.Join(", ", errors)}");
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("[Register] User not found from User.Identity");
                    return Unauthorized();
                }

                Console.WriteLine($"[Register] Starting registration for user: {user.Id}");
                Console.WriteLine($"[Register] Model - Name: {model.Name}, Description: {model.Description?.Length} chars");

                var success = await _organizationService.RegisterAsync(model, user.Id);

                Console.WriteLine($"[Register] RegisterAsync returned: {success}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Đăng ký tổ chức thành công! Bạn đã trở thành Ban tổ chức.";
                    return RedirectToAction(nameof(Success));
                }

                // Trường hợp này không nên xảy ra nữa vì service luôn throw exception
                Console.WriteLine("[Register] WARNING: RegisterAsync returned false without throwing exception!");
                ModelState.AddModelError("", "Đăng ký tổ chức thất bại. Vui lòng thử lại.");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[Register] InvalidOperationException: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Register] ArgumentException: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Register] Unexpected Exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Organization/Success
        public IActionResult Success()
        {
            return View();
        }
    }
}