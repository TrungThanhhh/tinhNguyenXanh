using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public OrganizationService(
            IOrganizationRepository repo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _repo = repo;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IEnumerable<OrganizationDTO>> GetAllAsync()
        {
            var orgs = await _repo.GetAllAsync();
            return orgs
                .Where(o => o.Verified)
                .Select(o => MapToDTO(o));
        }

        public async Task<OrganizationDTO?> GetByIdAsync(int id)
        {
            var o = await _repo.GetByIdAsync(id);
            if (o == null || !o.Verified) return null;

            return MapToDTO(o);
        }

        public async Task<bool> RegisterAsync(OrganizationDTO model, string userId)
        {
            try
            {
                // Validate input (bỏ AgreedToTerms)
                ValidateModel(model, userId);

                // Kiểm tra user tồn tại
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy user với ID: {userId}");
                }

                // Kiểm tra user đã có tổ chức chưa
                var existingOrg = await _repo.GetByUserIdAsync(userId);
                if (existingOrg != null)
                {
                    throw new InvalidOperationException("Bạn đã đăng ký tổ chức trước đó");
                }

                // Kiểm tra user đã là Organizer chưa
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(SD.Role_Organizer))
                {
                    throw new InvalidOperationException("Bạn đã là Ban tổ chức");
                }

                // Upload avatar nếu có
                string? avatarUrl = null;
                if (model.AvatarFile != null)
                {
                    avatarUrl = await UploadAvatarAsync(model.AvatarFile);
                }

                // Tạo organization mới
                var organization = new Organization
                {
                    UserId = userId,

                    // Thông tin cơ bản
                    Name = model.Name.Trim(),
                    OrganizationType = model.OrganizationType.Trim(),
                    Description = model.Description.Trim(),
                    FocusAreas = string.Join(",", model.FocusAreas),
                    AvatarUrl = avatarUrl, // Thêm avatar

                    // Thông tin liên hệ
                    ContactEmail = model.ContactEmail.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Website = model.Website?.Trim(),

                    // Địa chỉ
                    Address = model.Address.Trim(),
                    City = model.City.Trim(),
                    District = model.District.Trim(),
                    Ward = model.Ward?.Trim(),

                    // Thông tin pháp lý
                    TaxCode = model.TaxCode?.Trim(),
                    FoundedDate = model.FoundedDate,
                    LegalRepresentative = model.LegalRepresentative?.Trim(),

                    // Xác minh
                    VerificationDocsUrl = model.VerificationDocsUrl?.Trim(),
                    DocumentType = model.DocumentType?.Trim(),
                    Verified = true,

                    // Mạng xã hội
                    FacebookUrl = model.FacebookUrl?.Trim(),
                    ZaloNumber = model.ZaloNumber?.Trim(),

                    // Thống kê
                    MemberCount = model.MemberCount,
                    EventsOrganized = model.EventsOrganized,
                    Achievements = model.Achievements?.Trim(),

                    JoinedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };

                Console.WriteLine($"[RegisterAsync] Creating organization for userId: {userId}");
                Console.WriteLine($"[RegisterAsync] Organization: Name={organization.Name}, Avatar={avatarUrl}");

                // Lưu vào database
                await _repo.AddAsync(organization);
                await _repo.SaveChangesAsync();

                // Gán role Organizer
                Console.WriteLine($"[RegisterAsync] Adding role {SD.Role_Organizer} to user {userId}");
                var result = await _userManager.AddToRoleAsync(user, SD.Role_Organizer);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"[RegisterAsync] Failed to add role: {errors}");
                    throw new InvalidOperationException($"Không thể gán role Organizer: {errors}");
                }

                Console.WriteLine("[RegisterAsync] Registration completed successfully");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[RegisterAsync DbUpdateException] {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");

                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerMessage.Contains("FOREIGN KEY"))
                {
                    throw new InvalidOperationException("Lỗi liên kết dữ liệu. UserId không tồn tại trong hệ thống.");
                }
                else if (innerMessage.Contains("UNIQUE") || innerMessage.Contains("duplicate"))
                {
                    throw new InvalidOperationException("Tổ chức này đã tồn tại trong hệ thống.");
                }
                else if (innerMessage.Contains("NULL"))
                {
                    throw new InvalidOperationException("Thiếu thông tin bắt buộc. Vui lòng kiểm tra lại.");
                }
                else
                {
                    throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {innerMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterAsync Error] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Upload avatar
        private async Task<string> UploadAvatarAsync(IFormFile file)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)");
                }

                if (file.Length > 5 * 1024 * 1024) // 5MB
                {
                    throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");
                }

                // Create uploads folder
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "organizations");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/images/organizations/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadAvatarAsync Error] {ex.Message}");
                throw new InvalidOperationException($"Lỗi khi upload avatar: {ex.Message}");
            }
        }

        private void ValidateModel(OrganizationDTO model, string userId)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Tên tổ chức không được để trống");

            if (string.IsNullOrWhiteSpace(model.OrganizationType))
                throw new ArgumentException("Loại tổ chức không được để trống");

            if (string.IsNullOrWhiteSpace(model.Description) || model.Description.Length < 50)
                throw new ArgumentException("Mô tả phải có ít nhất 50 ký tự");

            if (model.FocusAreas == null || !model.FocusAreas.Any())
                throw new ArgumentException("Vui lòng chọn ít nhất một lĩnh vực hoạt động");

            if (string.IsNullOrWhiteSpace(model.ContactEmail))
                throw new ArgumentException("Email liên hệ không được để trống");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(model.Address))
                throw new ArgumentException("Địa chỉ không được để trống");

            if (string.IsNullOrWhiteSpace(model.City))
                throw new ArgumentException("Tỉnh/Thành phố không được để trống");

            if (string.IsNullOrWhiteSpace(model.District))
                throw new ArgumentException("Quận/Huyện không được để trống");

            // Bỏ validation AgreedToTerms

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId không hợp lệ");
        }

        private OrganizationDTO MapToDTO(Organization o)
        {
            return new OrganizationDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User?.UserName,

                // Thông tin cơ bản
                Name = o.Name,
                OrganizationType = o.OrganizationType,
                Description = o.Description,
                FocusAreas = string.IsNullOrEmpty(o.FocusAreas)
                    ? new List<string>()
                    : o.FocusAreas.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                AvatarUrl = o.AvatarUrl, // Thêm avatar

                // Thông tin liên hệ
                ContactEmail = o.ContactEmail,
                PhoneNumber = o.PhoneNumber,
                Website = o.Website,

                // Địa chỉ
                Address = o.Address,
                City = o.City,
                District = o.District,
                Ward = o.Ward,

                // Thông tin pháp lý
                TaxCode = o.TaxCode,
                FoundedDate = o.FoundedDate,
                LegalRepresentative = o.LegalRepresentative,

                // Xác minh
                VerificationDocsUrl = o.VerificationDocsUrl,
                DocumentType = o.DocumentType,
                Verified = o.Verified,

                // Mạng xã hội
                FacebookUrl = o.FacebookUrl,
                ZaloNumber = o.ZaloNumber,

                // Thống kê
                MemberCount = o.MemberCount,
                EventsOrganized = o.EventsOrganized,
                Achievements = o.Achievements,

                JoinedDate = o.JoinedDate
            };
        }
    }
}