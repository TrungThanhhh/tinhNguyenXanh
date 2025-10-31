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
            return orgs.Select(o => new OrganizationDTO
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
            if (o == null) return null;

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
