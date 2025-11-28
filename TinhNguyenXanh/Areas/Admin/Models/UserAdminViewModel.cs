namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class UserAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public bool IsLocked { get; set; }
        public string Role { get; set; } = "Volunteer"; // Admin, Organization, Volunteer
    }
}