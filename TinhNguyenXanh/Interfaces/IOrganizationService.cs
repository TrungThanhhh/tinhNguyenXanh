using TinhNguyenXanh.DTOs;

namespace TinhNguyenXanh.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDTO>> GetAllAsync();
        Task<OrganizationDTO?> GetByIdAsync(int id);
    }
}
