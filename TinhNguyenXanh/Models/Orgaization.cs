using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public bool Verified { get; set; } = false;
        public string VerificationDocsUrl { get; set; }
    }
}
