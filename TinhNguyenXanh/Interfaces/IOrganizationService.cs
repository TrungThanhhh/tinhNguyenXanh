using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDTO>> GetAllAsync();
        Task<OrganizationDTO?> GetByIdAsync(int id);

        Task<bool> RegisterAsync(OrganizationDTO model, string userId);
    }
}
