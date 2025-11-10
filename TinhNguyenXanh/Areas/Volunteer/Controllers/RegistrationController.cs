using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Volunteer.Controllers
{
    [Area("Volunteer")]
    [Authorize(Roles = "Volunteer")]
    public class RegistrationController : Controller
    {
        private readonly IEventRegistrationService _service;
        private readonly ApplicationDbContext _context;

        public RegistrationController(IEventRegistrationService service, ApplicationDbContext context)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null)
            {
                return RedirectToAction("CompleteProfile", "Profile");
            }

            var regs = await _service.GetByVolunteerIdAsync(volunteer.Id);
            return View(regs);
        }
    }
}