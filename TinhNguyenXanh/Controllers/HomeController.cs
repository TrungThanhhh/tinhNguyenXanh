using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.ViewModel;

namespace TinhNguyenXanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;

        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? keyword = "", int? category = null, string? location = "")
        {
            // Query sự kiện
            var eventQuery = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Where(e => e.Status == "approved");

            if (!string.IsNullOrWhiteSpace(keyword))
                eventQuery = eventQuery.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));

            if (category.HasValue && category.Value > 0)
                eventQuery = eventQuery.Where(e => e.CategoryId == category.Value);

            if (!string.IsNullOrWhiteSpace(location))
                eventQuery = eventQuery.Where(e => e.Location.Contains(location));

            var events = await eventQuery.ToListAsync();

            // Query tổ chức
            var orgQuery = _context.Organizations.Where(o => o.IsActive && o.Verified);

            if (!string.IsNullOrWhiteSpace(keyword))
                orgQuery = orgQuery.Where(o => o.Name.Contains(keyword) || o.Description.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(location))
                orgQuery = orgQuery.Where(o => o.Address.Contains(location));

            var organizations = await orgQuery.ToListAsync();

            var model = new SearchResultsViewModel
            {
                Keyword = keyword,
                CategoryId = category,
                Location = location,
                Events = events,
                Organizations = organizations
            };

            return View(model); // sẽ render ra Views/Home/Search.cshtml
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            var model = new ContactViewModel();

            if (User.Identity.IsAuthenticated)
            {
                model.Email = User.Identity.Name;
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            // Nếu đã đăng nhập → tự động điền email (giữ nguyên như cũ)
            if (User.Identity?.IsAuthenticated == true)
            {
                model.Email = User.Identity.Name ?? model.Email;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string adminEmail = "votaanlocc@gmail.com";
                string subject = $"[Liên hệ] {model.FullName} - {model.Subject ?? "Không có chủ đề"}";

                string body = $@"
            <h3 style='color:#16a34a;'>Có tin nhắn mới từ Tình Nguyện Xanh</h3>
            <hr>
            <p><strong>Người gửi:</strong> {model.FullName}</p>
            <p><strong>Email:</strong> <a href='mailto:{model.Email}'>{model.Email}</a></p>
            <p><strong>Số điện thoại:</strong> {model.Phone}</p>
            <p><strong>Chủ đề:</strong> {model.Subject ?? "Không có"}</p>
            <hr>
            <p><strong>Nội dung:</strong><br>{model.Message.Replace("\n", "<br>")}</p>
            <br>
            <small><em>Gửi lúc: {DateTime.Now:dd/MM/yyyy HH:mm}</em></small>
        ";

                // GỌI HÀM MỚI CÓ Reply-To → SIÊU QUAN TRỌNG!
                await _emailService.SendEmailAsync(
                    toEmail: adminEmail,
                    subject: subject,
                    message: body,
                    replyToEmail: model.Email,       // ← Người dùng sẽ nhận được khi bạn Reply
                    replyToName: model.FullName      // ← Tên người dùng hiện trong Reply-To
                );

                TempData["Success"] = "Gửi thành công! Chúng tôi đã nhận được tin nhắn và sẽ phản hồi bạn sớm nhất ";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                // Nếu lỗi gửi mail → vẫn hiện lỗi để biết
                TempData["Error"] = "Không thể gửi tin nhắn lúc này. Vui lòng thử lại sau!";
                // Ghi log để debug (nếu cần)
                Console.WriteLine("Lỗi gửi mail liên hệ: " + ex.Message);
                return View(model);
            }
        }

        //public async Task<IActionResult> TestEmail()
        //{
        //    await _emailService.SendEmailAsync("votaanlocc@gmail.com", "TEST MAIL THÀNH CÔNG!",
        //        "<h1 style='color:green'>XANH LÁ RỒI NÈ!</h1><p>Chúc mừng bạn đã gửi mail thành công!</p>");
        //    return Content("ĐÃ GỬI EMAIL THÀNH CÔNG! KIỂM TRA HỘP THƯ NGAY!");
        //}

        public IActionResult Donate()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
