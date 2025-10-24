using System.Collections.Generic;
using System.Threading.Tasks;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(int id);
    }
}
