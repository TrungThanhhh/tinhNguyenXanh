using TinhNguyenXanh.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace TinhNguyenXanh.Interfaces
{
    public class TopEventStatistic
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public int FavoriteCount { get; set; }
    }

    public interface IStatisticRepository
    {
        Task<int> GetTotalEventsAsync();
        Task<int> GetTotalVolunteersAsync();
        Task<IEnumerable<TopEventStatistic>> GetTopFavoriteEventsAsync(int count = 5);
    }
}
