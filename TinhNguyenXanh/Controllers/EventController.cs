using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Services;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;

        public EventController(IEventService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _service.GetAllEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound();
            return View(evt);
        }
    }
}
