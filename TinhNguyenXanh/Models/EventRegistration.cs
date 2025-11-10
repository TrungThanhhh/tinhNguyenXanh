namespace TinhNguyenXanh.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public string VolunteerId { get; set; } // UserId của Volunteer
        public Volunteer Volunteer { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "registered"; // registered/confirmed/cancelled
    }
}
    