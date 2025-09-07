using LandingApp.Models;

namespace LandingApp.Interfaces
{
    public interface ILeadService
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
