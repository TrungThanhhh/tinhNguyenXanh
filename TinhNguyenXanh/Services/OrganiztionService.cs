using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repo;

        public OrganizationService(IOrganizationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<OrganizationDTO>> GetAllAsync()
        {
            var orgs = await _repo.GetAllAsync();

            // 🔹 Chỉ lấy các tổ chức đã xác minh
            return orgs
                .Where(o => o.Verified)
                .Select(o => new OrganizationDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User?.UserName,
                    Name = o.Name,
                    Description = o.Description,
                    JoinedDate = o.JoinedDate,
                    Verified = o.Verified,
                    VerificationDocsUrl = o.VerificationDocsUrl
                });
        }

        public async Task<OrganizationDTO?> GetByIdAsync(int id)
        {
            var o = await _repo.GetByIdAsync(id);
            if (o == null || !o.Verified) return null; // 🔹 Chỉ cho phép xem nếu đã xác minh

            return new OrganizationDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User?.UserName,
                Name = o.Name,
                Description = o.Description,
                JoinedDate = o.JoinedDate,
                Verified = o.Verified,
                VerificationDocsUrl = o.VerificationDocsUrl
            };
        }
    }
}
