using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }
    }
}
