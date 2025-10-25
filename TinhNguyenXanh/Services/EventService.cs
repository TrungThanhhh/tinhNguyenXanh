using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.TinhNguyenXanh.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TinhNguyenXanh.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;

        public EventService(IEventRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _repo.GetAllEventsAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _repo.GetEventByIdAsync(id);
        }

        public async Task<bool> RegisterForEventAsync(int eventId, string userId)
        {
            // Lấy sự kiện từ repository
            var evt = await _repo.GetEventByIdAsync(eventId);
            if (evt == null || evt.Status != "approved") return false; // Chỉ đăng ký sự kiện approved

            // Lấy hoặc tạo Volunteer dựa trên userId
            var volunteer = await _repo.GetVolunteerByUserIdAsync(userId); // Giả sử repository có phương thức này
            if (volunteer == null)
            {
                volunteer = new Volunteer { UserId = userId, FullName = "Tên mặc định", JoinedDate = DateTime.UtcNow };
                await _repo.AddVolunteerAsync(volunteer); // Giả sử repository có phương thức thêm
            }

            // Kiểm tra đăng ký trùng lặp
            var existingReg = await _repo.GetRegistrationAsync(eventId, volunteer.Id.ToString());
            if (existingReg != null) return false;

            // Kiểm tra giới hạn số lượng
            var regCount = await _repo.GetRegistrationCountAsync(eventId);
            if (regCount >= evt.MaxVolunteers) return false;

            // Tạo và lưu đăng ký
            var registration = new EventRegistration
            {
                EventId = eventId,
                VolunteerId = volunteer.Id.ToString(),
                RegisteredDate = DateTime.UtcNow
            };
            await _repo.AddRegistrationAsync(registration);

            return true;
        }
    }
}