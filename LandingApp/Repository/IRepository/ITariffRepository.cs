using LandingApp.Models;

namespace LandingApp.Repository.IRepository
{
    public interface ITariffRepository
    {
        Task<TariffModel?> GetByIdAsync(int id);
        Task<IEnumerable<TariffModel>> GetAllAsync();
        Task AddAsync(TariffModel tariff);
        void Update(TariffModel tariff);
        void Delete(TariffModel tariff);
        Task SaveChangesAsync();

        // Дополнительно
        Task<TariffModel?> GetByNameAsync(string name);
    }
}
