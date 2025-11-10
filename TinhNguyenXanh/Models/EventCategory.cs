
namespace TinhNguyenXanh.Models
{
    public class EventCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
