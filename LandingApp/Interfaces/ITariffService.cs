using LandingApp.Models;

namespace LandingApp.Interfaces
{
    public interface ITariffService
    {
        Task<IEnumerable<TariffModel>> GetAllAsync();
        Task<TariffModel?> GetByIdAsync(int id);
        Task<TariffModel?> GetByNameAsync(string name);
        Task AddAsync(TariffModel tariff);
        Task UpdateAsync(TariffModel tariff);
        Task DeleteAsync(TariffModel tariff);
    }
}
