namespace TinhNguyenXanh.DTOs
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string? OrganizationName { get; set; }
        public string? CategoryName { get; set; }
        public int RegisteredCount { get; set; }
        public int MaxVolunteers { get; set; }
    }
}
