using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;

        public EventService(IEventRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _repo.GetAllEventsAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _repo.GetEventByIdAsync(id);
        }
    }
}
