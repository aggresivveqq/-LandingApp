using LandingApp.Models;
namespace LandingApp.Repository.IRepository
{
    public interface ILeadRepository
    {
        Task<IEnumerable<LeadModel>> GetAllAsync();
        Task<LeadModel?> GetByIdAsync(int id);
        Task AddAsync(LeadModel lead);
        Task DeleteAsync(int id);
        Task<LeadModel?> FindRecentLeadByPhoneAsync(string phone);
        Task<List<LeadModel>> GetUnexportedLeadsAsync();

        Task SaveChangesAsync();
    } 
}
