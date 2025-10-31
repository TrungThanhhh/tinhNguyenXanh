using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Areas.Organization.Controllers
{
    [Area("Organization")]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public async Task<IActionResult> Index()
        {
            var organizations = await _organizationService.GetAllAsync();
            return View(organizations);
        }
    }
}
