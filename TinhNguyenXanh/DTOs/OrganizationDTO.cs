namespace TinhNguyenXanh.DTOs
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; } // account của tổ chức
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool Verified { get; set; }
        public string? VerificationDocsUrl { get; set; }
    }
}
