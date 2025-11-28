using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using static TinhNguyenXanh.Interfaces.IStatisticRepository;

namespace TinhNguyenXanh.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly ApplicationDbContext _context;

        public StatisticRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalEventsAsync()
        {
            // Thống kê tổng số hoạt động (có thể thêm điều kiện lọc Status = 'Approved' hoặc tương tự)
            return await _context.Events.CountAsync();
        }

        public async Task<int> GetTotalVolunteersAsync()
        {
            // Thống kê tổng số tình nguyện viên (dựa trên bảng Volunteers)
            return await _context.Volunteers.CountAsync();
        }

        public async Task<IEnumerable<TopEventStatistic>> GetTopFavoriteEventsAsync(int count = 5)
        {
            // Truy vấn để tìm top N hoạt động có số lượt yêu thích cao nhất
            var topEvents = await _context.EventFavorites
                .GroupBy(ef => ef.EventId) // Nhóm theo EventId
                .Select(g => new
                {
                    EventId = g.Key,
                    FavoriteCount = g.Count() // Đếm số lượt yêu thích
                })
                .OrderByDescending(x => x.FavoriteCount)
                .Take(count)
                .Join(
                    _context.Events, // Join với bảng Events
                    stats => stats.EventId,
                    e => e.Id,
                    (stats, e) => new TopEventStatistic // Tạo DTO kết quả
                    {
                        EventId = stats.EventId,
                        Title = e.Title,
                        FavoriteCount = stats.FavoriteCount
                    }
                )
                .ToListAsync();

            return topEvents;
        }
    }
}
