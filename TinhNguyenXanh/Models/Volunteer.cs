using TinhNguyenXanh.Data;
namespace TinhNguyenXanh.Models
{
    public class Volunteer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public string Availability { get; set; }
        public bool IsPublic { get; set; } = true;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        // Mối quan hệ với kỹ năng, sở thích, đăng ký sự kiện...
        //public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        //public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    }
}
