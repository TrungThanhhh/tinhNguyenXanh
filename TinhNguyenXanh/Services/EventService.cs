using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;


namespace TinhNguyenXanh.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;
        public EventService(IEventRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IEnumerable<EventDTO>> GetAllEventsAsync()
        {
            var events = await _repo.GetAllEventsAsync();

            return events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
                Images = e.Images
            });
        }
        // Lấy các sự kiện của một tổ chức cụ thể
        public async Task<IEnumerable<EventDTO>> GetEventsByOrganizationAsync(int organizationId)
        {
            var events = await _repo.GetEventsByOrganizationIdAsync(organizationId);

            return events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
            });
        }
        public async Task<EventDTO?> GetEventByIdAsync(int id)
        {
            var e = await _repo.GetEventByIdAsync(id);
            if (e == null) return null;

            return new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers
            };
        }

        public async Task<IEnumerable<EventDTO>> GetApprovedEventsAsync()
        {
            var events = await _repo.GetAllEventsAsync();
            var approved = events
                .Where(e => e.Status?.Equals("approved", StringComparison.OrdinalIgnoreCase) == true);

            return approved.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
                Images = e.Images

            });
        }

        public async Task<bool> RegisterForEventAsync(int eventId, string userId)
        {
            var evt = await _repo.GetEventByIdAsync(eventId);
            if (evt == null || !evt.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                return false;

            var volunteer = await _repo.GetVolunteerByUserIdAsync(userId);
            if (volunteer == null)
            {
                volunteer = new Volunteer
                {
                    UserId = userId,
                    FullName = "Default Name",
                    JoinedDate = DateTime.UtcNow
                };
                await _repo.AddVolunteerAsync(volunteer);
            }

            var existingReg = await _repo.GetRegistrationAsync(eventId, volunteer.Id.ToString());
            if (existingReg != null)
                return false;

            var regCount = await _repo.GetRegistrationCountAsync(eventId);
            if (regCount >= evt.MaxVolunteers)
                return false;

            var registration = new EventRegistration
            {
                EventId = eventId,
                VolunteerId = volunteer.Id.ToString(),
                RegisteredDate = DateTime.UtcNow
            };
            await _repo.AddRegistrationAsync(registration);
            return true;
        }
        public async Task<bool> CreateEventAsync(EventDTO eventDto, int organizationId)
        {
            if (eventDto == null) return false;

            string? imagePath = null;

            if (eventDto.ImageFile != null && eventDto.ImageFile.Length > 0)
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(eventDto.ImageFile.FileName)}";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/events");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventDto.ImageFile.CopyToAsync(stream);
                    }
                    imagePath = $"/images/events/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IMAGE ERROR] {ex.Message}");
                    return false;
                }
            }

            var newEvent = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Location = eventDto.Location,
                LocationCoords = eventDto.LocationCoords,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                MaxVolunteers = eventDto.MaxVolunteers,
                OrganizationId = organizationId,
                CategoryId = eventDto.CategoryId,
                Status = "pending",
                Images = imagePath
            };

            try
            {
                await _repo.AddEventAsync(newEvent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[INNER] {ex.InnerException.Message}");
                return false;
            }
        }

    }
}
