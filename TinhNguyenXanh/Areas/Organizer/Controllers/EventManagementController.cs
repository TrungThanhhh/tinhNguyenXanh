using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class EventManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEventService _eventService;

    public EventManagementController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IEventService eventService)
    {
        _userManager = userManager;
        _context = context;
        _eventService = eventService;
    }
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.UserId == user.Id);

        if (organization == null)
            return RedirectToAction("Register", "Organization");

        var events = await _eventService.GetEventsByOrganizationAsync(organization.Id);
        return View(events);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _context.EventCategories.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventDTO model)
    {
        Console.WriteLine($"[DEBUG] Title: {model.Title}");
        Console.WriteLine($"[DEBUG] CategoryId: {model.CategoryId}");
        Console.WriteLine($"[DEBUG] ImageFile: {(model.ImageFile != null ? model.ImageFile.FileName : "NULL")}");
        Console.WriteLine($"[DEBUG] ModelState.IsValid: {ModelState.IsValid}");

        foreach (var state in ModelState)
            foreach (var error in state.Value.Errors)
                Console.WriteLine($"[VALIDATION ERROR] {state.Key}: {error.ErrorMessage}");

        ViewBag.Categories = new SelectList(await _context.EventCategories.ToListAsync(), "Id", "Name");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);

        if (organization == null)
        {
            ModelState.AddModelError("", "Không tìm thấy tổ chức của bạn.");
            return View(model);
        }

        var result = await _eventService.CreateEventAsync(model, organization.Id);
        if (!result)
        {
            ModelState.AddModelError("", "Không thể tạo sự kiện. Vui lòng thử lại.");
            return View(model);
        }

        TempData["Success"] = "Tạo sự kiện thành công! Đang chờ duyệt.";
        return RedirectToAction("Index");
    }

    public IActionResult Registrations()
    {
        return View();
    }
}