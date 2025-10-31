using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Organization?> GetByIdAsync(int id)
        {
            return await _context.Organizations
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
