using TinhNguyenXanh.Models;
namespace TinhNguyenXanh.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<Organization>> GetAllAsync();
        Task<Organization?> GetByIdAsync(int id);
    }
}
