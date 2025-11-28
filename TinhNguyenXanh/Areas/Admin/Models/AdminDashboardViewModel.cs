using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalVolunteers { get; set; }
        public int TotalReportsPending { get; set; }
        public int TotalOrganizations { get; set; }
        public int TotalUsers { get; set; }
        public IEnumerable<TopEventStatistic> Top5FavoriteEvents { get; set; } = new List<TopEventStatistic>();
    }

    public class AdminStatisticsViewModel : AdminDashboardViewModel
    {
        public int PendingReports { get; set; }
    }
}
